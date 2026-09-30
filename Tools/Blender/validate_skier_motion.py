"""Validate the existing rig in extreme poses and render comparison previews.

blender --background --python-exit-code 1 --python Tools/Blender/validate_skier_motion.py -- --output DevelopmentCaptures/Current/Blender
"""
import bpy, sys, json, math
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
from common.pipeline import ROOT, preview

def argument(name,default):
    return sys.argv[sys.argv.index(name)+1] if name in sys.argv else default
source=ROOT/argument('--source','ArtSource/Blender/Skier.blend')
output=ROOT/argument('--output','DevelopmentCaptures/Current/Blender');output.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.open_mainfile(filepath=str(source))
rig=bpy.data.objects['SkierRig'];objects=[o for o in bpy.context.scene.objects if o.type in ('MESH','ARMATURE')]
required=['Hips','Spine','Chest','Neck','Head']+[f'{bone}_{s}' for s in ['L','R'] for bone in ['UpperArm','LowerArm','Hand','Thigh','Shin','Foot','Ski','Pole']]
assert all(n in rig.data.bones for n in required)
for obj in objects:
    if obj.type!='MESH':continue
    for vert in obj.data.vertices:
        assert abs(sum(g.weight for g in vert.groups)-1)<.001,(obj.name,vert.index)
report={'source':str(source.relative_to(ROOT)),'bones':len(rig.data.bones),'renderers':len([o for o in objects if o.type=='MESH']),'poses':[]}
for label,action_name,frame in [('Crouch','Pose_Crouch',1),('Carve','Pose_CarveLeft',1),('AirTuck','Pose_Tuck',1),('Grab','Pose_Mute',1),('HardLanding','Pose_Landing',1)]:
    action=bpy.data.actions.get(action_name);assert action,action_name
    rig.animation_data.action=action;bpy.context.scene.frame_set(frame);bpy.context.view_layer.update()
    if label=='Grab':
        # Mirror the reachable runtime cross-body grab rather than the old generic source tuck.
        for s in ['L','R']:
            rig.pose.bones['Thigh_'+s].rotation_euler.x=math.radians(-150)
            rig.pose.bones['Shin_'+s].rotation_euler.x=math.radians(80)
            rig.pose.bones['Foot_'+s].rotation_euler.x=math.radians(-30)
        bpy.context.view_layer.update()
    largest=0;triangles=0
    deps=bpy.context.evaluated_depsgraph_get()
    for obj in objects:
        if obj.type!='MESH':continue
        evaluated=obj.evaluated_get(deps);mesh=evaluated.to_mesh();mesh.calc_loop_triangles()
        assert all(math.isfinite(c) for v in mesh.vertices for c in v.co)
        for edge in mesh.edges:
            a,b=edge.vertices;largest=max(largest,(mesh.vertices[a].co-mesh.vertices[b].co).length)
        triangles+=len(mesh.loop_triangles);evaluated.to_mesh_clear()
    assert triangles<15000 and largest<2.5,(label,triangles,largest)
    preview(output/(label+'.png'),objects)
    report['poses'].append({'pose':label,'triangles':triangles,'largestEdgeMeters':round(largest,4),'finite':True})
    rig.animation_data.action=None
    for bone in rig.pose.bones:bone.rotation_euler=(0,0,0)
(output/'validation.json').write_text(json.dumps(report,indent=2)+'\n')
print('SKIER MOTION VALIDATION PASS',json.dumps(report))
