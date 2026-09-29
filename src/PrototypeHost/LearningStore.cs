using Microsoft.Data.Sqlite;
namespace BilingualInput;

internal sealed class LearningStore(string path,PrivacyGate gate) {
 private SqliteConnection? Open(){
  if(!gate.LearningAllowed)return null;
  Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
  var db=new SqliteConnection(new SqliteConnectionStringBuilder{DataSource=path,Mode=SqliteOpenMode.ReadWriteCreate}.ToString());
  try{db.Open();using var cmd=db.CreateCommand();cmd.CommandText="""
   CREATE TABLE IF NOT EXISTS learning_items(
    key TEXT PRIMARY KEY, lemma TEXT NOT NULL, language TEXT NOT NULL, meaning TEXT NOT NULL,
    meaning_source TEXT NOT NULL, ipa TEXT NOT NULL, ipa_source TEXT NOT NULL,
    part_of_speech TEXT NOT NULL, pronunciation_reference TEXT NOT NULL,
    saved_at TEXT NOT NULL, encounter_count INTEGER NOT NULL DEFAULT 0
   );
   """;cmd.ExecuteNonQuery();return db;}catch{db.Dispose();throw;}
 }
 public bool Save(LearningItem item){
  if(!gate.LearningAllowed||item.Key.Length==0||item.Lemma.Length==0||item.MeaningSource.Length==0)return false;
  try{using var db=Open();if(db==null)return false;using var cmd=db.CreateCommand();cmd.CommandText="""
   INSERT INTO learning_items(key,lemma,language,meaning,meaning_source,ipa,ipa_source,part_of_speech,pronunciation_reference,saved_at,encounter_count)
   VALUES($key,$lemma,$language,$meaning,$source,$ipa,$ipaSource,$pos,$pronunciation,$savedAt,0)
   ON CONFLICT(key) DO NOTHING
   """;
   cmd.Parameters.AddWithValue("$key",item.Key);cmd.Parameters.AddWithValue("$lemma",item.Lemma);cmd.Parameters.AddWithValue("$language",item.Language);
   cmd.Parameters.AddWithValue("$meaning",item.Meaning);cmd.Parameters.AddWithValue("$source",item.MeaningSource);
   cmd.Parameters.AddWithValue("$ipa",item.Ipa);cmd.Parameters.AddWithValue("$ipaSource",item.IpaSource);
   cmd.Parameters.AddWithValue("$pos",item.PartOfSpeech);cmd.Parameters.AddWithValue("$pronunciation",item.PronunciationReference);
   cmd.Parameters.AddWithValue("$savedAt",item.SavedAt.ToString("O"));return cmd.ExecuteNonQuery()>0 || Contains(item.Key);
  }catch{return false;}
 }
 public bool Contains(string key){if(!gate.LearningAllowed)return false;try{using var db=Open();if(db==null)return false;using var cmd=db.CreateCommand();cmd.CommandText="SELECT 1 FROM learning_items WHERE key=$key";cmd.Parameters.AddWithValue("$key",key);return cmd.ExecuteScalar()!=null;}catch{return false;}}
 public bool Encounter(string key){if(!gate.LearningAllowed)return false;try{using var db=Open();if(db==null)return false;using var cmd=db.CreateCommand();cmd.CommandText="UPDATE learning_items SET encounter_count=encounter_count+1 WHERE key=$key";cmd.Parameters.AddWithValue("$key",key);return cmd.ExecuteNonQuery()>0;}catch{return false;}}
 public IReadOnlyList<LearningItem> All(){if(!gate.LearningAllowed)return [];try{using var db=Open();if(db==null)return [];using var cmd=db.CreateCommand();cmd.CommandText="SELECT key,lemma,language,meaning,meaning_source,ipa,ipa_source,part_of_speech,pronunciation_reference,saved_at,encounter_count FROM learning_items ORDER BY saved_at DESC";using var reader=cmd.ExecuteReader();var list=new List<LearningItem>();while(reader.Read())list.Add(new(reader.GetString(0),reader.GetString(1),reader.GetString(2),reader.GetString(3),reader.GetString(4),reader.GetString(5),reader.GetString(6),reader.GetString(7),reader.GetString(8),DateTimeOffset.Parse(reader.GetString(9)),reader.GetInt32(10)));return list;}catch{return [];}}
}
