using BilingualInput;
using System.Text;
static void Check(bool condition,string name){if(!condition)throw new Exception(name);Console.WriteLine("PASS "+name);}
var dir=Path.Combine(Path.GetTempPath(),"bi-learning-test-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);
try{
 var path=Path.Combine(dir,"learning.db");var gate=new PrivacyGate();var store=new LearningStore(path,gate);
 var item=new LearningItem("own-v004:refine","refine","EN","改进","own-v004","/ɹɪˈfaɪn/","CMUdict","verb","en-US:refine",DateTimeOffset.UtcNow,0);
 Check(store.Save(item),"save catalog lemma");Check(new LearningStore(path,gate).All().Single().Lemma=="refine","restart library read");
 var tracker=new EncounterTracker();Check(tracker.Observe("ref",item.Key,true),"first natural appearance");Check(!tracker.Observe("ref",item.Key,true),"redraw does not count");Check(!tracker.Observe("refine",item.Key,true),"continued composition does not count");tracker.Observe("",item.Key,true);Check(tracker.Observe("ref",item.Key,true),"new composition counts");
 Check(store.Encounter(item.Key)&&store.All().Single().EncounterCount==1,"persist encounter");
 var ticket=gate.Ticket();gate.Set(LearningPrivacy.Private);Check(!gate.Accept(ticket)&&!store.Encounter(item.Key)&&!store.Save(item)&&store.All().Count==0,"private blocks learning and stale task");
 gate.Set(LearningPrivacy.Secure);Check(!gate.CloudAllowed&&!gate.LearningAllowed&&!gate.ContentLoggingAllowed&&!gate.PersistentTextCacheAllowed,"secure gate");
 gate.Set(LearningPrivacy.Normal);Check(store.All().Single().EncounterCount==1,"private did not mutate saved data");
 var unavailable=new LearningStore(dir,gate);Check(!unavailable.Save(item)&&unavailable.All().Count==0,"database failure degrades");
 var sentinel="privacyuniquesentinelxfqk";tracker.Observe(sentinel,item.Key,false);
 using(var file=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite))using(var memory=new MemoryStream()){file.CopyTo(memory);Check(!Encoding.UTF8.GetString(memory.ToArray()).Contains(sentinel),"raw input absent from DB");}
}finally{Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();Directory.Delete(dir,true);}
