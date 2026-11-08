using UnityEngine;
using UnityEngine.UI;

namespace SpatialSurvivor {
public sealed class SurvivorEnemyHealthBar : MonoBehaviour {
    Canvas canvas;SurvivorGame owner;
    Image fill,trail;
    Text value,title;
    int lastHp=-1,lastMax=-1;
    float damageTrail=1;
    public string DisplayedHealth => value ? value.text : "";
    public float Fill => fill ? fill.rectTransform.localScale.x : 0;
    public bool Visible => canvas && canvas.gameObject.activeSelf;

    public void Initialize(SurvivorGame.Enemy enemy,Font font){
        owner=GetComponentInParent<SurvivorGame>();
        var root=new GameObject("Current HP",typeof(RectTransform),typeof(Canvas));
        root.transform.SetParent(transform,false);
        canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.sortingOrder=12;
        float width=enemy.type==4?320:188;
        root.GetComponent<RectTransform>().sizeDelta=new Vector2(width+8,70);
        Image Quad(string name,Color color,float height){
            var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));go.transform.SetParent(root.transform,false);
            var image=go.GetComponent<Image>();image.color=color;image.raycastTarget=false;
            image.rectTransform.sizeDelta=new Vector2(width,height);return image;
        }
        Quad("HP border",new Color(.04f,.07f,.12f,.94f),17).rectTransform.sizeDelta=new Vector2(width+6,17);
        trail=Quad("Recent damage",new Color(1,.74f,.35f),11);
        fill=Quad("Remaining health",enemy.type==4?new Color(.98f,.36f,.28f):new Color(.39f,.92f,.67f),11);
        foreach(var image in new[]{fill,trail}){image.rectTransform.pivot=new Vector2(0,.5f);image.rectTransform.anchoredPosition=new Vector2(-width/2,0);}
        Text Label(string name,float y,int size){
            var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Text));go.transform.SetParent(root.transform,false);
            var label=go.GetComponent<Text>();label.font=font;label.fontSize=size;label.alignment=TextAnchor.MiddleCenter;label.raycastTarget=false;
            label.rectTransform.sizeDelta=new Vector2(width+60,32);label.rectTransform.anchoredPosition=new Vector2(0,y);
            var shadow=go.AddComponent<Shadow>();shadow.effectColor=new Color(0,0,0,.9f);shadow.effectDistance=new Vector2(1,-1);
            return label;
        }
        value=Label("HP numbers",27,28);title=Label("Boss title",58,27);title.color=new Color(1,.81f,.42f);
        root.SetActive(false);
    }
    public void Refresh(SurvivorGame.Enemy enemy,Camera head,bool visible,string bossTitle){
        bool show=visible&&enemy.active&&enemy.hp>0&&head;
        canvas.gameObject.SetActive(show);if(!show)return;
        canvas.worldCamera=head;
        float fraction=Mathf.Clamp01((float)enemy.hp/Mathf.Max(1,enemy.maxHp));
        if(lastMax!=enemy.maxHp||enemy.hp>lastHp)damageTrail=fraction;
        if(lastHp!=enemy.hp||lastMax!=enemy.maxHp){value.text=Mathf.Max(0,enemy.hp)+" / "+enemy.maxHp;lastHp=enemy.hp;lastMax=enemy.maxHp;}
        fill.rectTransform.localScale=new Vector3(fraction,1,1);
        trail.rectTransform.localScale=new Vector3(damageTrail,1,1);
        bool focused=Vector3.Dot(head.transform.forward,(enemy.go.transform.position-head.transform.position).normalized)>.975f;title.gameObject.SetActive(enemy.type==4||enemy.elite||enemy.ward>0||focused||(owner&&owner.meta.showEnemyNames));title.text=enemy.type==4?bossTitle:(enemy.elite?"精英 · "+SurvivorQualityRules.AffixNames[(int)enemy.affix]+" · ":"")+SurvivorBestiary.Creatures[enemy.type].name+(enemy.ward>0?" [晶盾 "+Mathf.CeilToInt(enemy.ward)+"]": "");title.color=enemy.elite||enemy.type==4?new Color(1,.81f,.42f):new Color(.8f,.95f,.87f);fill.color=enemy.type==4?new Color(.98f,.36f,.28f):enemy.elite?new Color(1,.75f,.3f):new Color(.39f,.92f,.67f);
        float top=enemy.go.transform.position.y+(enemy.type==4?1.25f:.5f);
        var skin=enemy.visual?enemy.visual.GetComponent<SkinnedMeshRenderer>():null;
        // Animated renderer bounds can still contain the previous pooled spawn
        // or death pose. The source mesh gives a stable overhead anchor.
        if(skin&&skin.sharedMesh)top=enemy.go.transform.position.y+skin.sharedMesh.bounds.max.y*enemy.baseScale.y;
        Vector3 position=new Vector3(enemy.go.transform.position.x,top+.13f,enemy.go.transform.position.z);
        var direction=position-head.transform.position;
        canvas.transform.SetPositionAndRotation(position,Quaternion.LookRotation(direction.sqrMagnitude>.001f?direction:head.transform.forward));
        canvas.transform.localScale=Vector3.one*(.001f*Mathf.Clamp(direction.magnitude*.55f,.7f,2.4f));
    }
    public void Tick(float dt){if(lastMax>0)damageTrail=Mathf.MoveTowards(damageTrail,Mathf.Clamp01((float)lastHp/lastMax),dt*.7f);}
}
}
