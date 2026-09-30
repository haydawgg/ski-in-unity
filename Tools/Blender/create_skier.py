"""Original tailored skier. Dimensions/rig conventions remain in Unity metres."""
import bpy, math
from mathutils import Vector
from common.pipeline import *

def create_skier():
    clear()
    mats={name:material(name,color,rough) for name,color,rough in [
        ('skin',(.52,.32,.23),.7),('jacket',(.17,.50,.34),.8),
        ('jacket_accent',(.77,.86,.79),.75),('jacket_trim',(.075,.13,.17),.8),
        ('pants',(.12,.18,.25),.85),('pants_panel',(.18,.26,.34),.8),
        ('helmet',(.12,.18,.23),.4),('helmet_accent',(.77,.86,.79),.4),
        ('goggles',(.96,.49,.13),.12),('gloves',(.075,.10,.13),.8),
        ('boots',(.08,.12,.16),.4),('ski',(.045,.56,.62),.3),
        ('edges',(.34,.41,.46),.2),('ski_graphic',(.96,.78,.34),.35),
        ('ski_ink',(.045,.10,.16),.4),('equipment_trim',(.83,.89,.9),.4)]}
    arm=bpy.data.armatures.new('PowderFlowHumanoid');rig=bpy.data.objects.new('SkierRig',arm)
    bpy.context.collection.objects.link(rig);bpy.context.view_layer.objects.active=rig;rig.select_set(True);bpy.ops.object.mode_set(mode='EDIT')
    spec=[('Hips',None,(0,0,0),(0,.15,0)),('Spine','Hips',(0,.2,0),(0,.42,0)),('Chest','Spine',(0,.45,0),(0,.65,0)),('Neck','Chest',(0,.65,0),(0,.75,0)),('Head','Neck',(0,.78,0),(0,1.05,0))]
    for side,s in [(-1,'L'),(1,'R')]:
        spec.extend([(f'UpperArm_{s}','Chest',(side*.29,.48,0),(side*.34,.18,0)),(f'LowerArm_{s}',f'UpperArm_{s}',(side*.34,.18,0),(side*.36,-.10,.03)),(f'Hand_{s}',f'LowerArm_{s}',(side*.36,-.10,.03),(side*.36,-.20,.03)),(f'Thigh_{s}','Hips',(side*.14,-.1,0),(side*.16,-.42,0)),(f'Shin_{s}',f'Thigh_{s}',(side*.16,-.42,0),(side*.19,-.70,.03)),(f'Foot_{s}',f'Shin_{s}',(side*.19,-.7,.03),(side*.19,-.7,.20)),(f'Ski_{s}',f'Foot_{s}',(side*.19,-.78,0),(side*.19,-.78,.60)),(f'Pole_{s}',f'Hand_{s}',(side*.36,-.10,.03),(side*.36,-1.25,.03))])
    for name,parent,head,tail in spec:
        bone=arm.edit_bones.new(name);bone.head=v(head);bone.tail=v(tail)
        if parent:bone.parent=arm.edit_bones[parent]
    bpy.ops.object.mode_set(mode='OBJECT');rig.select_set(False)
    groups={'SkierClothing':[],'Ski_L':[],'Ski_R':[],'Pole_L':[],'Pole_R':[]}
    def attach(obj,bone,group='SkierClothing'):
        vg=obj.vertex_groups.new(name=bone);vg.add(list(range(len(obj.data.vertices))),1,'REPLACE')
        mod=obj.modifiers.new('Humanoid skin','ARMATURE');mod.object=rig;obj.parent=rig;groups[group].append(obj);return obj
    def weighted(obj,weights):
        names={n for pair in weights for n in pair};vg={n:obj.vertex_groups.new(name=n) for n in names}
        for i,pair in enumerate(weights):
            for n,w in pair.items():
                if w>0:vg[n].add([i],w,'REPLACE')
        mod=obj.modifiers.new('Humanoid skin','ARMATURE');mod.object=rig;obj.parent=rig;groups['SkierClothing'].append(obj);return obj
    def loft(name,rings,mat,weight,segments=24,square=.65):
        verts=[];faces=[];weights=[]
        for cx,y,cz,rx,rz in rings:
            for j in range(segments):
                a=j*math.tau/segments;c=math.cos(a);s=math.sin(a)
                x=cx+math.copysign(abs(c)**square,c)*rx;z=cz+math.copysign(abs(s)**square,s)*rz
                verts.append((x,y,z));weights.append(weight(y))
        for r in range(len(rings)-1):
            for j in range(segments):
                a=r*segments+j;b=r*segments+(j+1)%segments;faces.append((a,b,b+segments,a+segments))
        faces.extend([tuple(reversed(range(segments))),tuple(range((len(rings)-1)*segments,len(rings)*segments))])
        obj=mesh(name,verts,faces,mat)
        for p in obj.data.polygons:p.use_smooth=True
        return weighted(obj,weights)
    def split(y,center,span,above,below):
        t=max(0,min(1,(y-center+span)/(2*span)));return {above:t,below:1-t}

    # Tailored torso with broad shoulders, flat hem and a modest padded belly.
    torso=loft('TailoredJacket',[(0,y,z,rx,rz) for y,z,rx,rz in [
        (.055,0,.235,.17),(.075,0,.25,.18),(.12,.006,.25,.185),(.20,.006,.245,.185),
        (.29,0,.25,.185),(.36,0,.26,.18),(.43,0,.275,.18),(.49,-.004,.278,.177),
        (.54,-.008,.25,.16),(.59,-.006,.18,.125),(.625,0,.105,.095)]],mats['jacket'],lambda y:split(y,.43,.12,'Chest','Spine'),32)
    torso.data.materials.append(mats['jacket_accent'])
    for p in torso.data.polygons:
        cy=sum(torso.data.vertices[i].co.z for i in p.vertices)/len(p.vertices)
        if .43<cy<.55:p.material_index=1
    loft('PantsWaist',[(0,y,0,rx,rz) for y,rx,rz in [(.08,.225,.16),(0,.225,.175),(-.09,.23,.17),(-.16,.215,.15)]],mats['pants'],lambda y:{'Hips':1},24)
    attach(box('JacketHem',(0,.07,0),(.49,.032,.35),mats['jacket_trim'],.014),'Spine')
    attach(box('FrontZipper',(0,.34,.187),(.014,.43,.008),mats['equipment_trim'],.003),'Spine')
    attach(box('ZipperPull',(0,.50,.196),(.035,.035,.012),mats['jacket_trim'],.006),'Chest')
    for side,s in [(-1,'L'),(1,'R')]:
        attach(box('PocketWelt_'+s,(side*.14,.23,.191),(.11,.019,.012),mats['jacket_trim'],.007),'Spine')
    # A thick open collar/hood rim avoids a detached head/neck appearance.
    collar=loft('PaddedHood',[(0,.55,-.018,.12,.105),(0,.60,-.022,.15,.135),(0,.66,-.035,.15,.125),(0,.69,-.045,.12,.10)],mats['jacket_accent'],lambda y:{'Chest':1},32)
    attach(box('BackMountainBadge',(0,.34,-.19),(.15,.09,.015),mats['jacket_trim'],.015),'Spine')
    attach(mesh('OriginalPeakMark',[(-.06,.31,-.20),(0,.375,-.20),(.06,.31,-.20),(.02,.31,-.202),(0,.343,-.202),(-.02,.31,-.202)],[(0,1,2),(3,4,5)],mats['equipment_trim']),'Spine')
    attach(ellipsoid('HeadFace',(0,.80,.015),(.145,.185,.14),mats['skin'],20,12),'Head')
    attach(ellipsoid('HelmetShell',(0,.92,-.012),(.182,.145,.178),mats['helmet'],28,14),'Head')
    attach(box('HelmetRearPanel',(0,.925,-.179),(.19,.068,.017),mats['helmet_accent'],.015),'Head')
    for side,s in [(-1,'L'),(1,'R')]:
        for z in [-.07,.0,.065]:attach(box('HelmetVent_'+s+str(z),(side*.164,.96,z),(.015,.015,.035),mats['jacket_trim'],.005),'Head')
        attach(box('GoggleStrap_'+s,(side*.174,.84,-.025),(.025,.041,.26),mats['gloves'],.009),'Head')
    def lens(name,width,height,depth,mat,offset=0):
        verts=[];faces=[];n=16
        for i in range(n+1):
            x=-width*.5+width*i/n;z=.198+offset-.11*(x/.18)**2
            for zz in [z-depth,z]:
                for y in [.84-height*.5,.84+height*.5]:verts.append((x,y,zz))
        for i in range(n):
            a=i*4;b=a+4;faces.extend([(a,b,b+1,a+1),(a+2,a+3,b+3,b+2),(a,a+2,b+2,b),(a+1,b+1,b+3,a+3)])
        faces.extend([(0,1,3,2),(n*4,n*4+2,n*4+3,n*4+1)])
        return attach(mesh(name,verts,faces,mat),'Head')
    lens('GoggleFrame',.344,.124,.023,mats['equipment_trim']);lens('AmberLens',.315,.091,.005,mats['goggles'],.004)
    attach(box('HelmetBrow',(0,.91,.11),(.28,.025,.13),mats['helmet'],.012),'Head')

    for side,s in [(-1,'L'),(1,'R')]:
        sleeve=loft('ContinuousSleeve_'+s,[(side*x,y,z,rx,rz) for x,y,z,rx,rz in [
            (.26,.545,0,.10,.125),(.29,.49,0,.115,.13),(.305,.43,0,.115,.125),(.32,.34,0,.108,.12),
            (.335,.25,0,.105,.112),(.34,.18,0,.112,.116),(.345,.12,.005,.105,.108),(.35,.04,.015,.096,.102),
            (.355,-.04,.023,.087,.09),(.36,-.095,.03,.083,.086)]],mats['jacket'],lambda y:split(y,.18,.075,'UpperArm_'+s,'LowerArm_'+s),20,.9)
        sleeve.data.materials.append(mats['jacket_accent'])
        for p in sleeve.data.polygons:
            cy=sum(sleeve.data.vertices[i].co.z for i in p.vertices)/len(p.vertices)
            if .34<cy<.43:p.material_index=1
        attach(box('Cuff_'+s,(side*.36,-.08,.03),(.17,.044,.18),mats['jacket_trim'],.018),'LowerArm_'+s)
        attach(ellipsoid('Glove_'+s,(side*.36,-.16,.034),(.079,.10,.086),mats['gloves'],16,10),'Hand_'+s)
        attach(ellipsoid('GloveThumb_'+s,(side*.30,-.15,.064),(.035,.051,.037),mats['gloves'],12,8),'Hand_'+s)
        leg=loft('ContinuousPants_'+s,[(side*x,y,z,rx,rz) for x,y,z,rx,rz in [
            (.14,-.09,0,.12,.145),(.146,-.17,0,.133,.155),(.15,-.25,0,.128,.15),(.155,-.33,0,.123,.145),
            (.16,-.405,0,.126,.143),(.164,-.455,.005,.121,.137),(.17,-.51,.012,.119,.132),
            (.179,-.58,.02,.11,.123),(.187,-.65,.027,.10,.115),(.19,-.705,.03,.096,.106)]],mats['pants'],lambda y:split(y,-.42,.075,'Thigh_'+s,'Shin_'+s),24)
        leg.data.materials.append(mats['pants_panel'])
        for p in leg.data.polygons:
            cy=sum(leg.data.vertices[i].co.z for i in p.vertices)/len(p.vertices)
            if -.51<cy<-.405:p.material_index=1
        attach(box('Boot_'+s,(side*.19,-.68,.065),(.185,.21,.31),mats['boots'],.035),'Foot_'+s)
        attach(box('BootSole_'+s,(side*.19,-.771,.065),(.198,.025,.32),mats['jacket_trim'],.012),'Foot_'+s)
        for y in [-.61,-.67]:attach(box('BootBuckle_'+s+str(y),(side*.19,y,.195),(.15,.024,.018),mats['edges'],.008),'Foot_'+s)
        attach(box('BootPowerStrap_'+s,(side*.19,-.594,.05),(.19,.03,.20),mats['helmet_accent'],.012),'Foot_'+s)

        # Twin-tip outline, metal sidewalls and curved original graphics follow the actual ski surface.
        ski_group='Ski_'+s;verts=[];faces=[];segments=40
        def rise(z):return .09*max(0,(abs(z)/.85-.70)/.30)**2
        for i in range(segments+1):
            z=-.85+1.7*i/segments;t=abs(z)/.85;width=.048+.018*t*t
            for y in [-.80+rise(z),-.765+rise(z)]:
                for x in [-width,width]:verts.append((side*.19+x,y,z))
        for i in range(segments):
            a=i*4;b=a+4;faces.extend([(a,a+1,b+1,b),(a+2,b+2,b+3,a+3),(a,b,b+2,a+2),(a+1,a+3,b+3,b+1)])
        faces.extend([(0,2,3,1),(segments*4,segments*4+1,segments*4+3,segments*4+2)])
        ski=attach(mesh('TwinTip_'+s,verts,faces,mats['ski']),ski_group,ski_group);ski.data.materials.append(mats['edges'])
        for i,p in enumerate(ski.data.polygons):
            if i%4 in [2,3]:p.material_index=1
        def patch(name,z0,z1,half,mat,chevron=False):
            vv=[];ff=[];n=10
            for i in range(n+1):
                z=z0+(z1-z0)*i/n
                for x in [-half,half]:
                    zz=z+(abs(x)*1.5 if chevron else 0)
                    vv.append((side*.19+x,-.762+rise(zz),zz))
            for i in range(n):a=i*2;ff.append((a,a+1,a+3,a+2))
            attach(mesh(name,vv,ff,mat),ski_group,ski_group)
        patch('TailInk_'+s,-.83,-.51,.058,mats['ski_ink'])
        patch('TipArrow_'+s,.40,.66,.052,mats['ski_graphic'],True)
        patch('TipSnowLine_'+s,.65,.69,.051,mats['equipment_trim'],True)
        patch('TailSignature_'+s,-.71,-.66,.045,mats['equipment_trim'],True)
        for z in [.21,.27,.33]:patch('DirectionBars_'+s+str(z),z,z+.015,.036,mats['ski_ink'])
        attach(box('BindingHeel_'+s,(side*.19,-.728,-.09),(.14,.08,.105),mats['helmet'],.012),ski_group,ski_group)
        attach(box('BindingToe_'+s,(side*.19,-.726,.18),(.145,.075,.095),mats['helmet'],.012),ski_group,ski_group)
        for x in [-.047,.047]:attach(box('BindingPlate_'+s+str(x),(side*.19+x,-.751,.04),(.016,.02,.31),mats['edges'],.005),ski_group,ski_group)
        attach(box('HeelRelease_'+s,(side*.19,-.687,-.10),(.075,.012,.035),mats['equipment_trim'],.005),ski_group,ski_group)
        pole_group='Pole_'+s
        attach(rod('SkiPole_'+s,(side*.36,-.12,.03),(side*.36,-1.20,.03),.010,mats['edges'],12),pole_group,pole_group)
        attach(rod('PoleGrip_'+s,(side*.36,-.12,.03),(side*.36,-.28,.03),.021,mats['gloves'],12),pole_group,pole_group)
        attach(ellipsoid('PoleBasket_'+s,(side*.36,-1.12,.03),(.05,.009,.05),mats['helmet'],16,6),pole_group,pole_group)
        attach(rod('PoleAccent_'+s,(side*.36,-.32,.03),(side*.36,-.39,.03),.012,mats['ski_graphic'],12),pole_group,pole_group)
    # Five skinned renderers retain independent equipment bones while cutting primitive draw calls.
    for name,objects in groups.items():
        bpy.ops.object.select_all(action='DESELECT')
        for obj in objects:obj.select_set(True)
        bpy.context.view_layer.objects.active=objects[0];bpy.ops.object.join();visual_name=name if name=='SkierClothing' else 'Equipment'+name;bpy.context.object.name=visual_name;bpy.context.object.data.name=visual_name
    for name,bend,lean in [('Neutral',.18,0),('Crouch',.7,0),('Tuck',.9,0),('CarveLeft',.35,.3),('CarveRight',.35,-.3),('Takeoff',.5,0),('Airborne',.65,0),('Landing',1,0),('Rail',.6,0)]+[(g,1,0) for g in ['Safety','Mute','Japan','Tail','Nose','Stale','Blunt','Method']]:
        action=bpy.data.actions.new('Pose_'+name);rig.animation_data_create();rig.animation_data.action=action;action.use_fake_user=True
        for bone in rig.pose.bones:bone.rotation_mode='XYZ';bone.rotation_euler=(0,0,0)
        rig.pose.bones['Spine'].rotation_euler.x=bend*.4;rig.pose.bones['Hips'].rotation_euler.y=lean
        for s in ['L','R']:rig.pose.bones['Thigh_'+s].rotation_euler.x=-bend*.8;rig.pose.bones['Shin_'+s].rotation_euler.x=bend*1.4
        for bone in rig.pose.bones:bone.keyframe_insert(data_path='rotation_euler',frame=1)
    rig.animation_data.action=None
    for bone in rig.pose.bones:bone.rotation_euler=(0,0,0)
    return export('Skier','Character',{'seed':42,'heightMeters':1.85,'skiLength':1.7,'sourcePoses':17,'artRevision':'VP2','continuousGarments':True,'skinnedRenderers':5},True)

if __name__=='__main__':create_skier()
