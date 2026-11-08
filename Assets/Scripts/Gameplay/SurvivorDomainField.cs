using System.Collections.Generic;
using UnityEngine;

namespace SpatialSurvivor {
// All field artwork is batched into one small dynamic mesh per domain. Its
// simulation clock advances with combat, including during hit-stop and pause.
public sealed class SurvivorDomainField : MonoBehaviour {
    readonly List<Vector3> vertices = new List<Vector3>(4096);
    readonly List<Color> colors = new List<Color>(4096);
    readonly List<int> triangles = new List<int>(6144);
    Mesh mesh;
    Material material;
    SkillVisual kind;
    float clock, pulse, redraw;
    public float Radius { get; private set; }
    public float Clock => clock;
    public int VertexCount => mesh ? mesh.vertexCount : 0;
    bool Magnetic => kind == SkillVisual.MagneticField;
    bool Lightning => kind == SkillVisual.LightningField;
    bool Evolved => kind == SkillVisual.NetherPoison;
    Color Tint => Magnetic ? new Color(1,.77f,.37f,.5f) : Lightning ? new Color(.38f,.72f,1,.65f) : Evolved ? new Color(.38f,1,.69f,.60f) : new Color(.61f,.94f,.36f,.55f);

    public void Initialize(SkillVisual type) {
        kind = type;
        mesh = new Mesh { name = type + " animated sigil" };
        mesh.MarkDynamic();
        gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
        material = new Material(Resources.Load<Shader>("Shaders/Additive"));
        material.SetColor("_Color", Color.white);
        var renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        SetRadius(1.35f);
    }
    public void SetRadius(float radius) {
        radius = Mathf.Max(.05f,radius);
        if (Mathf.Abs(Radius-radius)<.001f) return;
        Radius = radius;
        Draw();
    }
    public void Restart() { clock=pulse=redraw=0; Draw(); }
    public void Pulse() { if(pulse>.95f)return; pulse=1; redraw=0; Draw(); }
    public void Tick(float dt) {
        clock+=dt; pulse=Mathf.Max(0,pulse-dt*3.5f); redraw-=dt;
        if (redraw>0) return;
        redraw=1f/30f;
        Draw();
    }
    Vector3 Point(float angle,float radius,float height=.025f) => new Vector3(Mathf.Cos(angle)*radius,height,Mathf.Sin(angle)*radius);
    void Stroke(Vector3 a,Vector3 b,float width,Color color) {
        Vector3 axis=Vector3.Cross(b-a,Vector3.up).normalized*width*.5f;
        if(axis.sqrMagnitude<.000001f)axis=Vector3.right*width*.5f;
        int i=vertices.Count;
        vertices.Add(a-axis);vertices.Add(a+axis);vertices.Add(b+axis);vertices.Add(b-axis);
        for(int n=0;n<4;n++)colors.Add(color);
        triangles.Add(i);triangles.Add(i+1);triangles.Add(i+2);
        triangles.Add(i);triangles.Add(i+2);triangles.Add(i+3);
    }
    void Arc(float radius,float start,float length,float width,Color color,int steps=64) {
        for(int i=0;i<steps;i++)Stroke(Point(start+length*i/steps,radius),Point(start+length*(i+1)/steps,radius),width,color);
    }
    void Draw() {
        if(!mesh)return;
        vertices.Clear();colors.Clear();triangles.Clear();
        var tint=Tint;
        float spin=clock*(Lightning?.16f:-.10f),tau=Mathf.PI*2;
        Arc(Radius,0,tau,.016f,tint);
        var faint=tint;faint.a=.23f;
        Arc(Radius*.89f,0,tau,.008f,faint);
        // Segmented outer band, engraved diamonds and orbiting rune strokes.
        int runes=Evolved?16:12;
        for(int i=0;i<runes;i++){
            float a=tau*i/runes+spin;
            Arc(Radius*.95f,a,.19f,.025f,tint,4);
            var p=Point(a,Radius*.80f);
            var radial=Point(a,.055f,0);var side=Point(a+Mathf.PI*.5f,.04f,0);
            Stroke(p-radial,p+side,.012f,tint);Stroke(p+side,p+radial,.012f,tint);
            Stroke(p+radial,p-side,.012f,tint);Stroke(p-side,p-radial,.012f,tint);
            Stroke(Point(a+.10f,Radius*.83f),Point(a+.10f,Radius*.87f),.009f,tint);
        }
        // A wave travels from the player to the exact gameplay radius.
        float wave=Mathf.Repeat(clock*(Lightning?.8f:.36f),1);
        var waveColor=tint;waveColor.a=(1-wave)*.28f+pulse*.22f;
        Arc(Radius*(.2f+.8f*wave),0,tau,.025f+pulse*.035f,waveColor);
        if(Magnetic){
            for(int i=0;i<8;i++){
                float a=tau*i/8;float r=Radius*(1-Mathf.Repeat(clock*.45f+i*.13f,1));
                var p=Point(a,r,.07f);var tip=p-Point(a,.1f,0);
                Stroke(p+Point(a+.7f,.06f,0),tip,.014f,tint);
                Stroke(tip,p+Point(a-.7f,.06f,0),.014f,tint);
            }
        }else if(Lightning){
            // Broken electrical filaments connect a six-point seal to its rim.
            for(int i=0;i<6;i++){
                float a=tau*i/6-spin;Vector3 last=Point(a,Radius*.38f);
                for(int j=1;j<=7;j++){
                    float r=Radius*(.38f+.56f*j/7);
                    var next=Point(a+Mathf.Sin(j*3.4f+Mathf.Floor(clock*12)+i)*.047f,r,.035f);
                    var electric=tint;electric.a=.18f+pulse*.65f;
                    Stroke(last,next,.012f+pulse*.009f,electric);last=next;
                }
                Stroke(Point(a,Radius*.38f),Point(a+tau/3,Radius*.38f),.012f,tint);
            }
        }else{
            // Low rising motes and curling leaf trails leave the eye line clear.
            int count=Evolved?30:18;
            for(int i=0;i<count;i++){
                float life=Mathf.Repeat(clock*.28f+i*.618034f,1);
                float a=i*2.39996f+clock*.18f;
                float r=Radius*(.32f+.58f*Mathf.Repeat(i*.381966f,1));
                float h=.035f+Mathf.Sin(life*Mathf.PI)*(Evolved?.38f:.22f);
                var p=Point(a,r,h);float s=.018f+Mathf.Sin(life*Mathf.PI)*.02f;
                var mote=tint;mote.a=Mathf.Sin(life*Mathf.PI)*.65f;
                Stroke(p-Vector3.right*s,p+Vector3.up*s,s,mote);
                Stroke(p+Vector3.up*s,p+Vector3.right*s,s,mote);
            }
            if(Evolved)for(int i=0;i<3;i++)for(int j=0;j<40;j++){
                float a=j*.12f+tau*i/3-spin;
                float r=Radius*(.28f+j*.012f);
                Stroke(Point(a,r),Point(a+.12f,r+Radius*.012f),.013f,faint);
            }
        }
        mesh.Clear();mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();
    }
    void OnDestroy(){if(mesh)Destroy(mesh);if(material)Destroy(material);}
}
}
