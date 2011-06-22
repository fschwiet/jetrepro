using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using NDesk.Options;
using Raven.Abstractions.Commands;
using Raven.Abstractions.Data;
using Raven.Abstractions.Extensions;
using Raven.Abstractions.Replication;
using Raven.Client.Document;
using Raven.Json.Linq;

namespace RavenConsole
{
    public class ReplicateCommand : AbstractCommand
    {
        public override string CommandText
        {
            get { return "begin-replicate"; }
        }

        public string Source;
        public string Target;
        public bool WaitForFinish;

        public override OptionSet GetOptionSet()
        {
            return new OptionSet
            {
                {"wait", "Wait until replicationg catches up", v => WaitForFinish = v != null}
            };
        }

        public override void HandleArgs(string[] remainingArgs)
        {
            remainingArgs = GetOptionSet().Parse(remainingArgs).ToArray();

            if (remainingArgs.Count() != 2)
                throw new ArgumentException("Incorrect number of arguments");

            Source = remainingArgs[0];
            Target = remainingArgs[1];
        }

        public override void Run()
        {
            var targetTest = HttpWebRequest.Create(Target);
            var response = targetTest.GetResponse() as HttpWebResponse;
            Guid sourceEtag = Guid.Empty;

            if (response.StatusCode != HttpStatusCode.OK || !response.ResponseUri.PathAndQuery.Contains("/raven/studio.html"))
                throw new ArgumentException("Target url doesn't look like a RavenDB.");

            using (var store = new DocumentStore() { Url = Source })
            {
                store.Initialize();

                using (var session = store.OpenSession())
                {
                    var replicationDestinationsId = "Raven/Replication/Destinations";

                    var destinations = session.Load<RavenJObject>(replicationDestinationsId);

                    if (destinations == null)
                        throw new Exception("Replication not detected at " + Source);

                    var targetArray = (destinations["Destinations"] as RavenJArray);

                    if (targetArray.Any(t => (t as RavenJObject)["Url"].Value<string>().Equals(Target)))
                    {
                        Console.WriteLine("Target is already a replication target");
                    }
                    else
                    {
                        session.Advanced.DatabaseCommands.Patch(
                            replicationDestinationsId, new PatchRequest[] {
                            new PatchRequest()
                            {
                                Name = "Destinations",
                                Type = PatchCommandType.Add,
                                Value = RavenJToken.FromObject(new ReplicationDestination() { 
                                    Url = Target
                                })}});

                        session.SaveChanges();
                    }
                }

                if (WaitForFinish)
                {
                    using (var session = store.OpenSession())
                    {
                        var tracer = new { TracerObjectToActAsReplicationSyncPoint = "Delete me later", Utc = DateTime.UtcNow, Local = DateTime.Now };

                        session.Store(tracer);
                        session.SaveChanges();
                         
                        sourceEtag = Guid.Parse(session.Advanced.GetMetadataFor(tracer).Value<string>("@etag"));

                        Console.WriteLine("Tracer written to source had etag: " + sourceEtag);
                    }

                    using(var session = store.OpenSession())
                    {
                        session.Store(new {hi = 123});
                        session.SaveChanges();
                    }
                }
            }

            var serializer = JsonExtensions.CreateDefaultJsonSerializer();


            if (WaitForFinish)

            {
                using (var webClient = new WebClient())
                do
                {
                    var replicationInfo = webClient.DownloadString(
                            new Uri(new Uri(Target),
                            "/replication/lastEtag?from=" + Source));

                    var replicationObject = serializer.Deserialize(new StringReader(replicationInfo), typeof(RavenJObject)) as RavenJObject;

                    RavenJToken lastEtag;

                    if (replicationObject.TryGetValue("LastDocumentEtag", out lastEtag))
                    {
                        var targetEtag = Guid.Parse(lastEtag.Value<string>());

                        if (targetEtag.CompareTo(sourceEtag) >= 0)
                            break;
                    }

                    Console.Write(".");
                    Thread.Sleep(1000);

                } while (true);
            }
        }


        public override string  RemainingArgsHelpText
        {
            get { return "<source> <target>"; }
        }
    }
}
