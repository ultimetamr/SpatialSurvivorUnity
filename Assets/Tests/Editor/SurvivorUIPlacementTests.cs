using NUnit.Framework;
using UnityEngine;
namespace SpatialSurvivor.Tests {
public class SurvivorUIPlacementTests {
 GameObject eye;Transform head;
 [SetUp]public void Setup(){eye=new GameObject("Placement eye");head=eye.transform;head.position=new Vector3(2,1.7f,-3);}
 [TearDown]public void Cleanup(){Object.DestroyImmediate(eye);}
 [Test]public void HudStartsBelowEyesAndFacesTheEye(){var p=new SurvivorUIPlacement();p.Tick(head,.01f);var offset=p.Position-head.position;Assert.That(offset.y,Is.EqualTo(-.48f).Within(.001f));Assert.That(new Vector2(offset.x,offset.z).magnitude,Is.EqualTo(1.6f).Within(.001f));Assert.That(Vector3.Angle(p.Rotation*Vector3.forward,offset),Is.LessThan(.1f));Assert.That(Vector3.Dot(p.Rotation*Vector3.right,Vector3.up),Is.EqualTo(0).Within(.001f));}
 [Test]public void LookingDownAndRollingDoesNotPushHudAway(){var p=new SurvivorUIPlacement();p.Tick(head,.01f);var before=p.Position;head.rotation=Quaternion.Euler(48,0,27);for(int i=0;i<120;i++)p.Tick(head,1/90f);Assert.That(Vector3.Distance(p.Position,before),Is.LessThan(.001f));Assert.That(p.Heading,Is.EqualTo(0).Within(.01f));}
 [Test]public void SmallGlancesLeaveHudStableAndLargeTurnsCatchUp(){var p=new SurvivorUIPlacement();p.Tick(head,.01f);head.rotation=Quaternion.Euler(0,18,0);for(int i=0;i<90;i++)p.Tick(head,1/90f);Assert.That(p.Heading,Is.EqualTo(0));head.rotation=Quaternion.Euler(0,90,0);for(int i=0;i<20;i++)p.Tick(head,1/90f);Assert.That(p.Heading,Is.EqualTo(0));for(int i=0;i<240;i++)p.Tick(head,1/90f);Assert.That(Mathf.DeltaAngle(p.Heading,90),Is.EqualTo(0).Within(.01f));}
 [Test]public void TrackingTeleportReanchorsWithoutFlyingThroughScene(){var p=new SurvivorUIPlacement();p.Tick(head,.01f);head.position+=Vector3.right*10;p.Tick(head,.01f);Assert.That(Vector3.Distance(p.Position,head.position+Vector3.forward*1.6f-Vector3.up*.48f),Is.LessThan(.001f));}
 [Test]public void NearVerticalGazeKeepsLastHeading(){var p=new SurvivorUIPlacement();head.rotation=Quaternion.Euler(0,74,0);p.Tick(head,.01f);head.rotation=Quaternion.Euler(90,0,0);for(int i=0;i<180;i++)p.Tick(head,1/90f);Assert.That(p.Heading,Is.EqualTo(74).Within(.01f));}
}
}
