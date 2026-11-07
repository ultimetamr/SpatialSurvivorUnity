using System;
using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
namespace SpatialSurvivor.Editor {
[InitializeOnLoad] public static class SurvivorVerification {
 static TestRunnerApi runner;
 static SurvivorVerification(){if(AssetDatabase.IsAssetImportWorkerProcess())return;runner=ScriptableObject.CreateInstance<TestRunnerApi>();runner.RegisterCallbacks(new Results());Application.logMessageReceived+=Capture;EditorApplication.update+=CheckRequest;}
 static void Capture(string message,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception){Directory.CreateDirectory("Logs");File.AppendAllText("Logs/verification-errors.txt",DateTime.UtcNow.ToString("O")+" "+message+"\n"+trace+"\n");}}
 static void CheckRequest(){if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;if(File.Exists("Logs/build-android.request")){File.Delete("Logs/build-android.request");try{SurvivorProject.BuildAndroid();File.WriteAllText("Logs/android-build-status.txt","Succeeded");}catch(Exception e){File.WriteAllText("Logs/android-build-status.txt",e.ToString());Debug.LogException(e);}return;}if(!File.Exists("Logs/run-verification.request"))return;File.Delete("Logs/run-verification.request");RunTests();}
 [MenuItem("SpatialSurvivor/Run Game Tests")]
 public static void RunTests(){Directory.CreateDirectory("Logs");File.WriteAllText("Logs/test-assembly-types.txt",string.Join("\n",Array.ConvertAll(Array.Find(AppDomain.CurrentDomain.GetAssemblies(),a=>a.GetName().Name=="SpatialSurvivor.Tests").GetTypes(),t=>t.FullName)));runner.Execute(new ExecutionSettings(new Filter{testMode=TestMode.EditMode,assemblyNames=new[]{"SpatialSurvivor.Tests"}}));}
 class Results:ICallbacks {public void RunStarted(ITestAdaptor tests){}public void TestStarted(ITestAdaptor test){}public void TestFinished(ITestResultAdaptor r){if(!r.HasChildren)File.AppendAllText("Logs/migration-tests.txt",r.FullName+" : "+r.ResultState+" "+r.Message+"\n");}public void RunFinished(ITestResultAdaptor r){File.WriteAllText("Logs/migration-tests-summary.json","{\"passed\":"+r.PassCount+",\"failed\":"+r.FailCount+",\"skipped\":"+r.SkipCount+"}");Debug.Log("Game tests: "+r.PassCount+" passed, "+r.FailCount+" failed");}}
}
}
