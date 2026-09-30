using UnityEngine;
namespace PowderFlow
{
    public readonly struct PresentationCanvas
    {
        public readonly float width,height,scale;public readonly Vector2 origin;public bool Portrait=>width<height;
        public PresentationCanvas(float screenWidth,float screenHeight,Rect safe)
        {
            bool portrait=screenWidth<screenHeight;scale=Mathf.Min(safe.width/(portrait?720:1280),safe.height/(portrait?1280:720));width=safe.width/scale;height=safe.height/scale;origin=new Vector2(safe.x,screenHeight-safe.yMax);
        }
        public Matrix4x4 Matrix=>Matrix4x4.TRS(origin,Quaternion.identity,Vector3.one*scale);
        public Rect[] SettingsRows(float margin,float rowHeight)
        {
            var result=new Rect[14];float gap=20,column=(width-margin*2-gap)/2,y=146;
            for(int i=0;i<14;i++)
            {
                if(!Portrait&&i==7)y=146;
                if(i==0||i==3||i==7)y+=28;
                result[i]=new Rect(Portrait?margin:margin+(i>=7?column+gap:0),y,Portrait?width-margin*2:column,rowHeight-3);y+=rowHeight;
            }
            return result;
        }
        public Rect FeedbackRect(bool session,float margin){float w=Mathf.Min(360,width*.5f-margin);return new Rect(width-margin-w,margin+(session?62:0),w,72);}
    }
    public sealed class Presentation
    {
        public readonly PresentationConfig config;public readonly GUIStyle title,body,small,metric,button;
        public Presentation(PresentationConfig value)
        {
            config=value;
            GUIStyle Style(int size,bool bold=false){var s=new GUIStyle(GUI.skin.label){font=bold?value.bold:value.regular,fontSize=size,wordWrap=true,padding=new RectOffset(0,0,0,0)};s.normal.textColor=value.text;return s;}
            title=Style(value.titleSize,true);body=Style(value.bodySize);small=Style(value.smallSize);small.normal.textColor=value.muted;metric=Style(value.metricSize,true);button=Style(value.bodySize,true);button.alignment=TextAnchor.MiddleLeft;
        }
        public void Fill(Rect rect,Color color){var old=GUI.color;GUI.color=old*color;GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=old;}
        public void Panel(Rect rect,bool border=true){Fill(rect,config.panel);if(border){Fill(new Rect(rect.x,rect.y,rect.width,1),config.line);Fill(new Rect(rect.x,rect.yMax-1,rect.width,1),config.line);}}
        public void Text(Rect rect,string text,GUIStyle style,Color? color=null){var old=style.normal.textColor;if(color.HasValue)style.normal.textColor=color.Value;GUI.Label(rect,text,style);style.normal.textColor=old;}
        public void ShadowText(Rect rect,string text,GUIStyle style,Color color)
        {
            var shadow=rect;shadow.position+=Vector2.one*config.hudShadowOffset;Text(shadow,text,style,config.hudShadow);Text(rect,text,style,color);
        }
        public bool Stepper(Rect rect,string text)
        {
            Fill(rect,config.panel);var old=button.alignment;button.alignment=TextAnchor.MiddleCenter;Text(rect,text,button,config.focus);button.alignment=old;return GUI.Button(rect,GUIContent.none,GUIStyle.none);
        }
        public bool Button(Rect rect,string text,bool active,string detail=null)
        {
            Fill(rect,active?Color.Lerp(config.card,config.focus,.18f):config.card);Fill(new Rect(rect.x,rect.y,active?4:1,rect.height),active?config.focus:config.line);
            var content=new Rect(rect.x+18,rect.y+(detail==null?0:7),rect.width-48,detail==null?rect.height:28);Text(content,text,button,active?config.focus:config.text);
            if(detail!=null)Text(new Rect(rect.x+18,rect.y+34,rect.width-48,22),detail,small);
            Text(new Rect(rect.xMax-32,rect.y+(rect.height-24)/2,24,24),active?"›":"",body,config.focus);
            return GUI.Button(rect,GUIContent.none,GUIStyle.none);
        }
    }
}
