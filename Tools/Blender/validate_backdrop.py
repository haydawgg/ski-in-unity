"""Inspect the five existing non-colliding range sources and their snow coverage."""
import bpy,sys,json,math
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
from common.pipeline import ROOT
output=ROOT/(sys.argv[sys.argv.index('--output')+1] if '--output' in sys.argv else 'DevelopmentCaptures/AfterReferencePass/backdrop-validation.json')
report=[]
for variant in range(5):
    source=ROOT/f'ArtSource/Blender/Ridge{variant}.blend';bpy.ops.wm.open_mainfile(filepath=str(source))
    objects=[o for o in bpy.context.scene.objects if o.type=='MESH'];assert len(objects)==1
    mesh=objects[0].data;mesh.calc_loop_triangles();assert len(mesh.loop_triangles)<20000
    assert all(math.isfinite(c) for vertex in mesh.vertices for c in vertex.co)
    area=sum(p.area for p in mesh.polygons);snow=sum(p.area for p in mesh.polygons if p.material_index==1)
    upward=sum(p.area for p in mesh.polygons if p.normal.z>0)/area
    if '--require-quality' in sys.argv:assert upward>.99 and .08<snow/area<.9,(variant,upward,snow/area)
    report.append({'name':f'Ridge{variant}','triangles':len(mesh.loop_triangles),'upwardAreaFraction':round(upward,4),'snowAreaFraction':round(snow/area,4),'finite':True})
output.parent.mkdir(parents=True,exist_ok=True);output.write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
