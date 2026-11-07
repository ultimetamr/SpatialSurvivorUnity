using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace SpatialSurvivor {
public static class LegacyProgressMigration {
 static readonly string[] Keys={"starting_max_health_level","starting_attack_damage_level","starting_attack_speed_level","starting_attack_range_level","starting_pickup_range_level","starting_experience_gain_level","crystal_multiplier_level","wave_reward_level"};
 public static bool TryRead(string xml,out MetaSave save,out string error){save=null;error="";if(string.IsNullOrWhiteSpace(xml)||xml.Length>65536){error="原版存档为空或过大";return false;}try{var result=new MetaSave();bool hasMoney=false;var settings=new XmlReaderSettings{DtdProcessing=DtdProcessing.Prohibit,XmlResolver=null,MaxCharactersInDocument=65536};using(var reader=XmlReader.Create(new StringReader(xml),settings)){reader.MoveToContent();if(reader.Name!="map")throw new FormatException("不是 Android SharedPreferences 存档");while(reader.Read()){if(reader.NodeType!=XmlNodeType.Element||reader.Name!="int")continue;string key=reader.GetAttribute("name");if(!int.TryParse(reader.GetAttribute("value"),NumberStyles.Integer,CultureInfo.InvariantCulture,out int value)||value<0)throw new FormatException("存档数值无效");if(key=="total_crystals"){result.crystals=value;hasMoney=true;}int index=Array.IndexOf(Keys,key);if(index>=0)result.levels[index]=Math.Min(value,MetaSave.Caps[index]);}}if(!hasMoney)throw new FormatException("缺少 total_crystals，未导入");using(var hash=SHA256.Create())result.legacyImportHash=BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(xml))).Replace("-","");save=result;return true;}catch(Exception ex){error=ex.Message;return false;}}
 public static bool MergeOnce(MetaSave current,string xml,out string message){if(!TryRead(xml,out var incoming,out message))return false;if(!string.IsNullOrEmpty(current.legacyImportHash)){message="原版进度已导入过，避免重复导入晶核";return false;}current.crystals=Math.Max(current.crystals,incoming.crystals);for(int i=0;i<8;i++)current.levels[i]=Math.Max(current.levels[i],incoming.levels[i]);current.legacyImportHash=incoming.legacyImportHash;message="原版永久强化与晶核已导入，下局生效";return true;}
}
}
