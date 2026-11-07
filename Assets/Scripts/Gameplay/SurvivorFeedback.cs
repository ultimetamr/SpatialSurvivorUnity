using System;
using UnityEngine;
using UnityEngine.XR;

namespace SpatialSurvivor {
public enum FeedbackCue { Shot,Hit,Kill,Pickup,LevelUp,Select,Hurt,Boss,Victory,Defeat,Explosion,Freeze,Purchase }

// Audio is synthesized once, with a bounded voice pool. No allocations in the combat feedback path.
public sealed class SurvivorFeedback:MonoBehaviour {
 readonly AudioSource[] voices=new AudioSource[10];readonly AudioClip[] clips=new AudioClip[13];readonly float[] nextAllowed=new float[13];
 float nextHaptic;int voice;SurvivorGame game;
 public bool SoundEnabled {get=>game.meta.soundEnabled;set{game.meta.soundEnabled=value;game.Save();if(!value)Stop();}}
 public bool HapticsEnabled {get=>game.meta.hapticsEnabled;set{game.meta.hapticsEnabled=value;game.Save();}}
 void Awake(){game=GetComponent<SurvivorGame>();for(int i=0;i<voices.Length;i++){var go=new GameObject("Feedback Voice "+i);go.transform.SetParent(transform,false);voices[i]=go.AddComponent<AudioSource>();voices[i].playOnAwake=false;voices[i].spatialBlend=1;voices[i].minDistance=.6f;voices[i].maxDistance=12;voices[i].rolloffMode=AudioRolloffMode.Linear;voices[i].dopplerLevel=0;}
  for(int i=0;i<clips.Length;i++)clips[i]=Generate((FeedbackCue)i);
 }
 static AudioClip Generate(FeedbackCue cue){const int rate=22050;float duration=cue==FeedbackCue.Victory?1.15f:cue==FeedbackCue.Defeat?.8f:cue==FeedbackCue.LevelUp?.5f:cue==FeedbackCue.Hurt?.16f:.12f;int count=(int)(rate*duration);var samples=new float[count];var random=new System.Random(120+(int)cue);for(int i=0;i<count;i++){float t=(float)i/rate,p=t/duration,frequency=220;switch(cue){case FeedbackCue.Shot:frequency=Mathf.Lerp(1000,240,p);break;case FeedbackCue.Hit:frequency=150;break;case FeedbackCue.Kill:frequency=Mathf.Lerp(350,100,p);break;case FeedbackCue.Pickup:frequency=Mathf.Lerp(800,1300,p);break;case FeedbackCue.LevelUp:case FeedbackCue.Victory:frequency=new[]{523.25f,659.25f,783.99f,1046.5f}[Mathf.Min(3,(int)(p*4))];break;case FeedbackCue.Defeat:frequency=Mathf.Lerp(310,90,p);break;case FeedbackCue.Hurt:frequency=100;break;case FeedbackCue.Boss:frequency=75;break;case FeedbackCue.Select:case FeedbackCue.Purchase:frequency=660;break;case FeedbackCue.Explosion:frequency=65;break;case FeedbackCue.Freeze:frequency=Mathf.Lerp(1400,900,p);break;}float wave=Mathf.Sin(2*Mathf.PI*frequency*t);if(cue==FeedbackCue.Hit||cue==FeedbackCue.Hurt||cue==FeedbackCue.Explosion)wave=.35f*wave+.65f*((float)random.NextDouble()*2-1);samples[i]=wave*Mathf.Min(1,t*180)*Mathf.Pow(1-p,2)*.25f;}var clip=AudioClip.Create("Survivor "+cue,count,1,rate,false);clip.SetData(samples,0);return clip;}
 public void Play(FeedbackCue cue,Vector3 position){int index=(int)cue;float now=Time.unscaledTime;if(now<nextAllowed[index])return;nextAllowed[index]=now+(cue==FeedbackCue.Hit?.08f:cue==FeedbackCue.Pickup?.06f:.04f);if(SoundEnabled){AudioSource selected=null;foreach(var source in voices)if(!source.isPlaying){selected=source;break;}if(selected==null)selected=voices[voice++%voices.Length];selected.Stop();selected.transform.position=position;selected.clip=clips[index];selected.volume=cue==FeedbackCue.Hit?.3f:.7f;selected.Play();}if(HapticsEnabled&&(cue==FeedbackCue.Hurt||cue==FeedbackCue.Select||cue==FeedbackCue.LevelUp||cue==FeedbackCue.Boss||cue==FeedbackCue.Victory||cue==FeedbackCue.Purchase))Pulse(cue==FeedbackCue.Hurt?.6f:.25f,cue==FeedbackCue.Hurt?.12f:.06f);}
 void Pulse(float amplitude,float duration){if(Time.unscaledTime<nextHaptic)return;nextHaptic=Time.unscaledTime+.08f;Send(XRNode.RightHand,amplitude,duration);Send(XRNode.LeftHand,amplitude,duration);}
 static void Send(XRNode node,float amplitude,float duration){var device=InputDevices.GetDeviceAtXRNode(node);if(device.isValid&&device.TryGetHapticCapabilities(out var capabilities)&&capabilities.supportsImpulse)device.SendHapticImpulse(0,amplitude,duration);}
 public void Stop(){foreach(var source in voices)if(source)source.Stop();}
 void OnApplicationFocus(bool value){if(!value)Stop();}
 void OnDestroy(){foreach(var clip in clips)if(clip)Destroy(clip);}
}
}
