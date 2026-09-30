using UnityEngine;
namespace PowderFlow
{
    public partial class GameFlow
    {
        Presentation ui;OutfitPreview preview;
        internal Event reviewPointer;
        public Rect[] SettingRectangles(PresentationCanvas canvas)=>canvas.SettingsRows(PresentationConfig.Active.margin,PresentationConfig.Active.settingRow);
        void OnGUI()
        {
            if(!menu)return;if(ui==null)ui=new Presentation(PresentationConfig.Active);
            var c=new PresentationCanvas(Screen.width,Screen.height,Screen.safeArea);var old=GUI.matrix;GUI.matrix=c.Matrix;var t=ui.config;float m=t.margin,w=c.width,h=c.height;
            var oldEvent=Event.current;bool injected=oldEvent.type==EventType.Repaint&&reviewPointer!=null;
            if(injected){Event.current=reviewPointer;reviewPointer=null;}
            try
            {
                ui.Fill(new Rect(0,0,w,h),new Color(.025f,.045f,.07f,.25f));
                if(page=="main")MainPage(c);
                else
                {
                    ui.Panel(new Rect(m-12,m-12,w-m*2+24,h-m*2+24));
                    ui.Text(new Rect(m,40,w-m*2,22),"POWDERFLOW  /  "+(page=="results"?"SCORE SESSION":page.ToUpperInvariant()),ui.small,t.focus);
                    ui.Text(new Rect(m,72,w-m*2,60),page=="settings"?"Make it yours.":page=="controls"?"Find your rhythm.":page=="outfit"?"Your mountain kit.":"Session complete.",ui.title);
                    if(page=="settings")SettingsPage(c);else if(page=="controls")ControlsPage(c);else if(page=="outfit")OutfitPage(c);else ResultsPage(c);
                }
            }
            finally{if(injected)Event.current=oldEvent;GUI.matrix=old;}
        }
        void MainPage(PresentationCanvas c)
        {
            float m=ui.config.margin,width=Mathf.Min(580,c.width-m*2),x=c.Portrait?(c.width-width)/2:m,y=c.Portrait?150:44;
            ui.Panel(new Rect(x-18,y-18,width+36,620));
            ui.Text(new Rect(x,y,width,24),startInMenu?"FREESTYLE / HIGH ALPINE":"TAKE A BREATH",ui.small,ui.config.focus);
            ui.Text(new Rect(x,y+34,width,60),startInMenu?"POWDERFLOW":"PAUSED",ui.title);
            ui.Text(new Rect(x,y+96,width,36),startInMenu?"Find your line. Keep the flow.":world.Area,ui.body,ui.config.muted);
            y+=158;
            var labels=startInMenu?new[]{"Free ride","Score session","Settings & outfit","Controls","Quit"}:new[]{"Resume","Restart at top","Restart at park","Settings & outfit","Return to menu","Quit"};
            var details=startInMenu?new[]{"Explore the mountain at your own pace","150 seconds to put a line together","Sound, controls, display and your kit","A few moves. Endless combinations.","See you on the next lap"}:new[]{"Keep the line flowing","A fresh lap from the ridge","Drop straight into the feature line","Sound, controls, display and your kit","Back to the mountain menu","See you on the next lap"};
            float step=startInMenu?74:65,row=startInMenu?64:57;
            for(int i=0;i<labels.Length;i++){var rect=new Rect(x,y+i*step,width,row);Hover(rect,i);if(ui.Button(rect,labels[i],selected==i,details[i]))Activate(i);}
            ui.Text(new Rect(x,y+labels.Length*step+12,width,24),"PERSONAL BEST   "+SaveStore.Current.highScore.ToString("N0"),ui.small,ui.config.warm);
            ui.Panel(new Rect(m,c.height-m-36,c.width-m*2,36),false);
            ui.Text(new Rect(m+12,c.height-m-28,c.width-m*2-24,28),"↑↓ / D-pad  Select     Enter / A  Confirm     Esc / B  Back",ui.small);
        }
        void Hover(Rect r,int index){if(pointerMode&&r.Contains(Event.current.mousePosition))selected=index;}
        void SettingsPage(PresentationCanvas c)
        {
            var s=SaveStore.Current;float m=ui.config.margin;
            string[] names={"Master","Effects","Music","Keyboard","Gamepad","Camera","Field of view","Quality","Lighting","Outfit","Fullscreen","Resolution","Speed units","Invert camera"};
            string[] values={s.master.ToString("P0"),s.sfx.ToString("P0"),s.music.ToString("P0"),s.keyboardSensitivity.ToString("F1"),s.gamepadSensitivity.ToString("F1"),s.cameraSensitivity.ToString("F1"),s.fov.ToString("F0")+"°",new[]{"Low","Medium","High","Epic"}[s.quality],s.day?"Day":"Sunset",world.catalog.characterVisuals.outfits[s.outfit].name,s.fullscreen?"On":"Off",s.width+" × "+s.height,s.mph?"mph":"km/h",s.invertCamera?"On":"Off"};
            var rows=SettingRectangles(c);
            for(int i=0;i<rows.Length;i++)
            {
                var r=rows[i];if(i==0||i==3||i==7)ui.Text(new Rect(r.x,r.y-25,r.width,22),i==0?"AUDIO":i==3?"CONTROLS":"DISPLAY & OUTFIT",ui.small,ui.config.focus);
                Hover(r,i);ui.Fill(r,selected==i?Color.Lerp(ui.config.card,ui.config.focus,.14f):ui.config.card);if(selected==i)ui.Fill(new Rect(r.x,r.y,3,r.height),ui.config.focus);
                if(ui.Stepper(new Rect(r.x+8,r.y+4,32,r.height-8),"−")) {selected=i;Adjust(i,-1);}
                ui.Text(new Rect(r.x+50,r.y+11,r.width*.46f-46,27),names[i],ui.body);
                ui.Text(new Rect(r.x+r.width*.47f,r.y+11,r.width*.53f-50,27),values[i],ui.body,selected==i?ui.config.focus:ui.config.text);
                if(ui.Stepper(new Rect(r.xMax-40,r.y+4,32,r.height-8),"+")){selected=i;Adjust(i,1);}
            }
            float bottom=c.height-m-82;
            if(ui.Button(new Rect(m,bottom,(c.width-m*2-18)/2,48),"View outfit",false)){page="outfit";selected=0;}
            var save=new Rect(c.width/2+9,bottom,(c.width-m*2-18)/2,48);Hover(save,14);if(ui.Button(save,"Save & back",selected==14))Back();
            ui.Text(new Rect(m,c.height-m-20,c.width-m*2,28),"↑↓ Select   ←→ Adjust   Enter / A Save   O / Y Outfit   Esc / B Back",ui.small);
        }
        void ControlsPage(PresentationCanvas c)
        {
            float m=ui.config.margin,gap=18,width=c.Portrait?c.width-m*2:(c.width-m*2-gap)/2;
            string[] headings={"BUILD SPEED","TAKE FLIGHT","GRAB & TWEAK","RESET & EXPLORE"};
            string[] copy={"A / D · left stick — carve\nW / Y — tuck\nS / X — brake\nHold Space / A, release — pop","A / D · left stick — spin\n↑ / ↓ · sticks — flip\n← / → · right stick — roll\nHold right mouse — look around","Q / E · triggers — grabs\nShift / Ctrl · bumpers — styles\nBoth grab keys — cross skis\nHold a grab to add style and points","R / B — quick retry\nT / D-pad up — set marker\nY / D-pad down — retry marker\nEsc / Start — pause · L — lighting"};
            for(int i=0;i<4;i++)
            {
                float x=m+(c.Portrait?0:i%2*(width+gap)),y=160+(c.Portrait?i:i/2)*190;
                ui.Fill(new Rect(x,y,width,176),ui.config.card);ui.Text(new Rect(x+18,y+16,width-36,24),headings[i],ui.small,ui.config.focus);ui.Text(new Rect(x+18,y+48,width-36,115),copy[i],ui.body);
            }
            if(ui.Button(new Rect(m,c.height-m-72,c.width-m*2,48),"Back to menu",true))Back();
            ui.Text(new Rect(m,c.height-m-18,c.width-m*2,24),"Enter / A  Confirm     Esc / B  Back     F1  Telemetry     F2  Traces",ui.small);
        }
        void OutfitPage(PresentationCanvas c)
        {
            if(!preview){preview=gameObject.AddComponent<OutfitPreview>();preview.Initialize(world.catalog);}
            float m=ui.config.margin;var view=c.Portrait?new Rect((c.width-500)/2,155,500,640):new Rect(m,150,360,450);
            ui.Fill(view,ui.config.card);if(preview.Image)GUI.DrawTexture(view,preview.Image,ScaleMode.ScaleToFit);
            if(Event.current.type==EventType.MouseDrag&&view.Contains(Event.current.mousePosition)){preview.Rotate(Event.current.delta.x);Event.current.Use();}
            float x=c.Portrait?m:440,y=c.Portrait?825:200,width=c.Portrait?c.width-m*2:c.width-440-m;
            var outfit=world.catalog.characterVisuals.outfits[SaveStore.Current.outfit];
            ui.Text(new Rect(x,y,width,60),outfit.name,ui.title);ui.Text(new Rect(x,y+62,width,26),"OUTFIT "+(SaveStore.Current.outfit+1)+" / 6",ui.small,ui.config.focus);
            var colors=new[]{outfit.jacket,outfit.pants,outfit.skis,outfit.accent};for(int i=0;i<4;i++)ui.Fill(new Rect(x+i*58,y+110,42,12),colors[i]);
            if(ui.Button(new Rect(x,y+154,(width-14)/2,48),"Previous",false))Adjust(9,-1);
            if(ui.Button(new Rect(x+(width+14)/2,y+154,(width-14)/2,48),"Next",false))Adjust(9,1);
            ui.Text(new Rect(x,y+218,width,60),"←→ / D-pad  Choose a kit\nDrag the preview to turn",ui.small);
            if(ui.Button(new Rect(m,c.height-m-72,c.width-m*2,48),"Back to settings",true))Back();
            ui.Text(new Rect(m,c.height-m-18,c.width-m*2,24),"Your selection is saved with settings.     Enter / A / B  Back",ui.small);
        }
        void ResultsPage(PresentationCanvas c)
        {
            float m=ui.config.margin;ui.Text(new Rect(m,190,c.width-m*2,70),world.player.GetComponent<ComboSystem>().Total.ToString("N0")+" points",ui.title,ui.config.warm);
            ui.Text(new Rect(m,282,c.width-m*2,42),"PERSONAL BEST   "+SaveStore.Current.highScore.ToString("N0"),ui.body);
            if(ui.Button(new Rect(m,c.height-m-72,c.width-m*2,48),"Continue",true)){ScoreSession=false;Back();}
        }
    }
}
