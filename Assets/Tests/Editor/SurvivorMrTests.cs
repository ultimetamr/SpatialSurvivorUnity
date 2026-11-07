using System;
using NUnit.Framework;
using UnityEngine;

namespace SpatialSurvivor.Tests {
public class SurvivorMrTests {
 [TestCase(SurvivorMrPhase.Booting)]
 [TestCase(SurvivorMrPhase.Permission)]
 [TestCase(SurvivorMrPhase.Starting)]
 [TestCase(SurvivorMrPhase.Loading)]
 [TestCase(SurvivorMrPhase.Scanning)]
 [TestCase(SurvivorMrPhase.Denied)]
 [TestCase(SurvivorMrPhase.Failed)]
 public void IncompleteMrNeverAllowsCombatEvenWithAnOldNavigationGrid(SurvivorMrPhase phase){var state=new SurvivorMrState();state.Set(phase,"test");Assert.That(state.CanPlay(true,true),Is.False);}
 [Test] public void RoomTrackingAndMrMustAllBeReady(){var state=new SurvivorMrState();state.Set(SurvivorMrPhase.Running,"ready");Assert.That(state.CanPlay(true,true),Is.True);Assert.That(state.CanPlay(false,true),Is.False);Assert.That(state.CanPlay(true,false),Is.False);state.Set(SurvivorMrPhase.Scanning,"scan");Assert.That(state.CanPlay(true,true),Is.False);state.Set(SurvivorMrPhase.Running,"cancelled scan");Assert.That(state.CanPlay(true,true),Is.True);}
 [TestCase(false)][TestCase(true)]
 public void ConcaveRoomTriangulationPreservesBoundaryAndWinding(bool reverse){
  var p=new[]{new Vector2(0,0),new Vector2(4,0),new Vector2(4,1),new Vector2(1,1),new Vector2(1,4),new Vector2(0,4)};
  if(reverse)Array.Reverse(p);var triangles=SurvivorRoomGeometry.Triangulate(p);Assert.That(triangles.Length,Is.EqualTo(24));
  float area=0;for(int i=0;i<triangles.Length;i+=6){var a=p[triangles[i]];var b=p[triangles[i+1]];var c=p[triangles[i+2]];area+=Mathf.Abs((b.x-a.x)*(c.y-a.y)-(b.y-a.y)*(c.x-a.x))*.5f;var center=(a+b+c)/3;Assert.That(center.x<=1.001f||center.y<=1.001f,Is.True,"Room recess must remain outside geometry");}
  Assert.That(area,Is.EqualTo(7).Within(.0001f));
 }
 [Test] public void DegeneratePolygonIsRejected(){Assert.That(SurvivorRoomGeometry.Triangulate(new[]{Vector2.zero,Vector2.right,Vector2.right*2}),Is.Empty);}
 [Test] public void ClosedScannedPolygonAcceptsRepeatedAndCollinearBoundaryPoints(){
  var p=new[]{new Vector2(0,0),new Vector2(2,0),new Vector2(4,0),new Vector2(4,0),new Vector2(4,1),new Vector2(1,1),new Vector2(1,4),new Vector2(0,4),new Vector2(0,0)};
  var indices=SurvivorRoomGeometry.Triangulate(p);Assert.That(indices.Length,Is.EqualTo(24));float area=0;for(int i=0;i<indices.Length;i+=6){var a=p[indices[i]];var b=p[indices[i+1]];var c=p[indices[i+2]];area+=Mathf.Abs((b.x-a.x)*(c.y-a.y)-(b.y-a.y)*(c.x-a.x))*.5f;}Assert.That(area,Is.EqualTo(7).Within(.0001f));
 }
 [Test] public void MeshUpdatesReplaceCollisionAndRemovalInvalidatesRoom(){
  var parent=new GameObject("MR test origin");var material=new Material(Resources.Load<Shader>("Shaders/RoomOcclusion"));
  var room=new SurvivorRoomGeometry(parent.transform,"Scanned test room",material);var id=Guid.NewGuid();
  try {
   var floor=new[]{new Vector3(-3,0,-3),new Vector3(-3,0,3),new Vector3(3,0,3),new Vector3(3,0,-3)};var indices=new[]{0,1,2,0,2,3};
   Assert.That(room.Upsert(id,Vector3.zero,Quaternion.identity,floor,indices),Is.True);Physics.SyncTransforms();
   var nav=new RoomNavigation();nav.Rebuild(new Vector3(0,1.65f,0));Assert.That(nav.Ready,Is.True);Assert.That(nav.Floor,Is.EqualTo(0).Within(.01));
   // The origin transform must apply equally to collider poses and tracking poses.
   parent.transform.position=new Vector3(10,0,0);Physics.SyncTransforms();Assert.That(Physics.Raycast(new Vector3(10,2,0),Vector3.down,out var hit,3,1<<RoomNavigation.Layer),Is.True);Assert.That(hit.point.y,Is.EqualTo(0).Within(.01));
   Assert.That(room.Upsert(id,Vector3.up*.3f,Quaternion.identity,floor,indices),Is.True);Physics.SyncTransforms();nav.Rebuild(new Vector3(10,1.95f,0));Assert.That(nav.Floor,Is.EqualTo(.3f).Within(.01));Assert.That(room.Count,Is.EqualTo(1));Assert.That(room.Triangles,Is.EqualTo(2));
   Assert.That(room.Upsert(id,Vector3.zero,Quaternion.identity,floor,new[]{0,1,9}),Is.False);Assert.That(room.Triangles,Is.EqualTo(2));
   room.Remove(id);Physics.SyncTransforms();nav.Rebuild(new Vector3(10,1.95f,0));Assert.That(nav.Ready,Is.False);Assert.That(room.Count,Is.Zero);Assert.That(room.Triangles,Is.Zero);
  } finally {room.Dispose();UnityEngine.Object.DestroyImmediate(parent);UnityEngine.Object.DestroyImmediate(material);}
 }
}
}
