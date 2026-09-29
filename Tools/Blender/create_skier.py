import bpy,math
from mathutils import Vector
from common.pipeline import *

def create_skier():
    clear();skin=material('skin',(.52,.32,.23));jacket=material('jacket',(.16,.42,.29));pants=material('pants',(.12,.13,.22));black=material('helmet',(.035,.045,.06));goggle=material('goggles',(.9,.47,.16),.12);glove=material('gloves',(.04,.045,.06));skiMat=material('ski',(.05,.56,.58),.32);edge=material('edges',(.23,.28,.31),.2);boot=material('boots',(.075,.085,.10));graphic=material('ski_graphic',(.91,.70,.40))
    arm=bpy.data.armatures.new('PowderFlowHumanoid');rig=bpy.data.objects.new('SkierRig',arm);bpy.context.collection.objects.link(rig);bpy.context.view_layer.objects.active=rig;rig.select_set(True);bpy.ops.object.mode_set(mode='EDIT')
    spec=[('Hips',None,(0,0,0),(0,.15,0)),('Spine','Hips',(0,.2,0),(0,.42,0)),('Chest','Spine',(0,.45,0),(0,.65,0)),('Neck','Chest',(0,.65,0),(0,.75,0)),('Head','Neck',(0,.78,0),(0,1.05,0))]
    for side,s in [(-1,'L'),(1,'R')]:
        spec.extend([(f'UpperArm_{s}','Chest',(side*.29,.48,0),(side*.34,.18,0)),(f'LowerArm_{s}',f'UpperArm_{s}',(side*.34,.18,0),(side*.36,-.10,.03)),(f'Hand_{s}',f'LowerArm_{s}',(side*.36,-.10,.03),(side*.36,-.20,.03)),(f'Thigh_{s}','Hips',(side*.14,-.1,0),(side*.16,-.42,0)),(f'Shin_{s}',f'Thigh_{s}',(side*.16,-.42,0),(side*.19,-.70,.03)),(f'Foot_{s}',f'Shin_{s}',(side*.19,-.7,.03),(side*.19,-.7,.20)),(f'Ski_{s}',f'Foot_{s}',(side*.19,-.78,0),(side*.19,-.78,.60)),(f'Pole_{s}',f'Hand_{s}',(side*.36,-.10,.03),(side*.36,-1.25,.03))])
    for name,parent,head,tail in spec:
        bone=arm.edit_bones.new(name);bone.head=v(head);bone.tail=v(tail)
        if parent:bone.parent=arm.edit_bones[parent]
    bpy.ops.object.mode_set(mode='OBJECT');rig.select_set(False)
    def attach(obj,bone):
        group=obj.vertex_groups.new(name=bone);group.add(list(range(len(obj.data.vertices))),1,'REPLACE');mod=obj.modifiers.new('Humanoid skin','ARMATURE');mod.object=rig;obj.parent=rig;return obj
    attach(ellipsoid('JacketBody',(0,.35,0),(.29,.34,.19),jacket,32,16),'Spine')
    attach(ellipsoid('PantsHips',(0,-.03,0),(.25,.16,.18),pants),'Hips')
    attach(ellipsoid('HeadFace',(0,.79,.02),(.155,.20,.15),skin),'Head')
    attach(ellipsoid('HelmetShell',(0,.9,-.015),(.18,.16,.18),black,32,16),'Head')
    attach(box('GogglesLens',(0,.83,.158),(.30,.10,.05),goggle,.025),'Head')
    attach(box('GoggleStrap',(0,.84,0),(.33,.035,.31),glove,.01),'Head')
    attach(box('JacketPocket',(0,.34,.18),(.31,.12,.04),jacket,.02),'Spine')
    for side,s in [(-1,'L'),(1,'R')]:
        attach(rod('UpperSleeve_'+s,(side*.29,.48,0),(side*.34,.18,0),.10,jacket,20),'UpperArm_'+s)
        attach(rod('LowerSleeve_'+s,(side*.34,.18,0),(side*.36,-.10,.03),.09,jacket,20),'LowerArm_'+s)
        attach(ellipsoid('Glove_'+s,(side*.36,-.14,.03),(.08,.10,.085),glove),'Hand_'+s)
        attach(ellipsoid('BaggyThigh_'+s,(side*.15,-.26,0),(.13,.22,.14),pants),'Thigh_'+s)
        attach(ellipsoid('BaggyShin_'+s,(side*.175,-.53,0),(.11,.18,.12),pants),'Shin_'+s)
        attach(box('Boot_'+s,(side*.19,-.68,.055),(.19,.22,.32),boot,.035),'Foot_'+s)
        # Closed twin-tip ski: symmetric waist, wide tips, smooth rise at both ends.
        verts=[];faces=[];segments=36
        for i in range(segments+1):
            z=-.85+1.7*i/segments;t=abs(z)/.85;width=.048+.018*t*t;rise=.09*max(0,(t-.70)/.30)**2
            for y in [-.80+rise,-.765+rise]:
                for x in [-width,width]:verts.append((side*.19+x,y,z))
        for i in range(segments):
            a=i*4;b=a+4;faces.extend([(a,a+1,b+1,b),(a+2,b+2,b+3,a+3),(a,b,a+2+4,a+2),(a+1,a+3,b+3,b+1)])
        faces.extend([(0,2,3,1),(segments*4,segments*4+1,segments*4+3,segments*4+2)])
        attach(mesh('TwinTip_'+s,verts,faces,skiMat),'Ski_'+s)
        for offset in [-.4,.38]:attach(box('SkiGraphic_'+s+str(offset),(side*.19,-.758,offset),(.10,.003,.15),graphic),'Ski_'+s)
        attach(box('Binding_'+s,(side*.19,-.745,.04),(.13,.06,.29),black,.015),'Ski_'+s)
        attach(rod('SkiPole_'+s,(side*.36,-.12,.03),(side*.36,-1.20,.03),.012,edge,10),'Pole_'+s)
        attach(ellipsoid('PoleBasket_'+s,(side*.36,-1.12,.03),(.06,.012,.06),black,12,6),'Pole_'+s)
    # Reusable source pose library remains in the blend; gameplay drives bones procedurally.
    for name,bend,lean in [('Neutral',0,0),('Crouch',.7,0),('Tuck',.9,0),('CarveLeft',.35,.3),('CarveRight',.35,-.3),('Takeoff',.5,0),('Airborne',.65,0),('Landing',1,0),('Rail',.6,0)]+[(g,1,0) for g in ['Safety','Mute','Japan','Tail','Nose','Stale','Blunt','Method']]:
        action=bpy.data.actions.new('Pose_'+name);rig.animation_data_create();rig.animation_data.action=action;action.use_fake_user=True
        for bone in rig.pose.bones:bone.rotation_mode='XYZ';bone.rotation_euler=(0,0,0)
        rig.pose.bones['Spine'].rotation_euler.x=bend*.4;rig.pose.bones['Hips'].rotation_euler.y=lean
        for s in ['L','R']:rig.pose.bones['Thigh_'+s].rotation_euler.x=-bend*.8;rig.pose.bones['Shin_'+s].rotation_euler.x=bend*1.4
        for bone in rig.pose.bones:bone.keyframe_insert(data_path='rotation_euler',frame=1)
    rig.animation_data.action=None
    for bone in rig.pose.bones:bone.rotation_euler=(0,0,0)
    return export('Skier','Character',{'seed':42,'heightMeters':1.85,'skiLength':1.7,'sourcePoses':17},True)
if __name__=='__main__':create_skier()
