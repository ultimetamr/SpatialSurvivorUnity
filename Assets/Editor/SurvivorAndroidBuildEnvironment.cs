using System.IO;
using System.Text.RegularExpressions;
using UnityEditor.Android;
using UnityEngine;

namespace SpatialSurvivor.Editor {
public sealed class SurvivorAndroidBuildEnvironment : IPostGenerateGradleAndroidProject {
 public int callbackOrder=>int.MaxValue-1;
 public void OnPostGenerateGradleAndroidProject(string path){
  var properties=Path.Combine(Directory.GetParent(Path.GetFullPath(path)).FullName,"gradle.properties");
  var temp=Path.GetFullPath(Path.Combine(Application.dataPath,"../Logs/java-sockets")).Replace('\\','/');
  Directory.CreateDirectory(temp);
  var text=File.ReadAllText(properties);
  // A shared daemon may retain another Unity project's deleted TEMP path.
  // Give this build a distinct JVM configuration and use a single-use daemon.
  const string pattern=@"(?m)^org\.gradle\.jvmargs=(.*)$";
  if(Regex.IsMatch(text,pattern))text=Regex.Replace(text,pattern,m=>
   "org.gradle.jvmargs="+Regex.Replace(m.Groups[1].Value.Trim(),@"\s*-Djava\.io\.tmpdir=\S+","")+" -Djava.io.tmpdir="+temp);
  else text+="\norg.gradle.jvmargs=-Xmx4096m -Djava.io.tmpdir="+temp+"\n";
  text=Regex.Replace(text,@"(?m)^org\.gradle\.daemon=.*\r?\n?","");
  File.WriteAllText(properties,text+"\norg.gradle.daemon=false\n");
 }
}
}
