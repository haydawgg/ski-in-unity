"""Original deterministic mesh pipeline. Public inputs/metadata use Unity x/y/z meters."""
import bpy, math, json, random, datetime
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[3]
OUT=ROOT/'Assets/Art/Generated'
SOURCE=ROOT/'ArtSource/Blender'
MATERIALS={}
def v(p): return (-p[0],-p[2],p[1])
def clear():
    bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False);MATERIALS.clear()
def material(name,color,roughness=.65):
    if name in MATERIALS:return MATERIALS[name]
    m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True
    bs=m.node_tree.nodes.get('Principled BSDF');bs.inputs['Base Color'].default_value=(*color,1);bs.inputs['Roughness'].default_value=roughness
    MATERIALS[name]=m;return m
def box(name,pos,size,mat,bevel=0):
    bpy.ops.mesh.primitive_cube_add(size=1,location=v(pos));o=bpy.context.object;o.name=name;o.dimensions=(size[0],size[2],size[1]);bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    if bevel:
        mod=o.modifiers.new('Soft edges','BEVEL');mod.width=bevel;mod.segments=2;bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=mod.name)
    o.data.materials.append(mat);return o
def ellipsoid(name,pos,size,mat,segments=24,rings=12):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments,ring_count=rings,radius=1,location=v(pos));o=bpy.context.object;o.name=name;o.scale=(size[0],size[2],size[1]);bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(mat)
    for p in o.data.polygons:p.use_smooth=True
    return o
def rod(name,a,b,radius,mat,sides=12):
    delta=Vector(v(b))-Vector(v(a));middle=(Vector(v(a))+Vector(v(b)))*.5
    bpy.ops.mesh.primitive_cylinder_add(vertices=sides,radius=radius,depth=delta.length,location=middle);o=bpy.context.object;o.name=name;o.rotation_euler=delta.to_track_quat('Z','Y').to_euler();bpy.ops.object.transform_apply(location=False,rotation=True,scale=True);o.data.materials.append(mat)
    for p in o.data.polygons:p.use_smooth=True
    return o
def mesh(name,vertices,faces,mat):
    data=bpy.data.meshes.new(name);data.from_pydata([v(p) for p in vertices],[],faces);data.update();o=bpy.data.objects.new(name,data);bpy.context.collection.objects.link(o);data.materials.append(mat)
    bpy.context.view_layer.objects.active=o;o.select_set(True)
    if data.polygons:
        bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.mesh.normals_make_consistent(inside=False);bpy.ops.object.mode_set(mode='OBJECT')
    o.select_set(False)
    # Deterministic planar UVs suitable for procedural/triplanar materials too.
    uv=data.uv_layers.new(name='UVMap')
    for poly in data.polygons:
        for loop in poly.loop_indices:
            co=data.vertices[data.loops[loop].vertex_index].co;uv.data[loop].uv=(co.x*.25,co.y*.25)
    return o
def validate(objects):
    tris=0;bounds=[]
    for o in objects:
        if o.type!='MESH':continue
        assert len(o.data.vertices)>0 and len(o.data.polygons)>0,o.name
        assert all(math.isfinite(c) for vert in o.data.vertices for c in vert.co),o.name
        assert max(abs(s-1) for s in o.scale)<.001,o.name
        if not o.data.uv_layers:
            uv=o.data.uv_layers.new(name='UVMap')
            for loop in o.data.loops:
                co=o.data.vertices[loop.vertex_index].co;uv.data[loop.index].uv=(co.x*.3,co.z*.3)
        o.data.calc_loop_triangles();tris+=len(o.data.loop_triangles)
        bounds.extend(o.matrix_world@Vector(p) for p in o.bound_box)
    assert tris>0 and tris<4000000,tris
    return tris,[[min(p[i] for p in bounds) for i in range(3)],[max(p[i] for p in bounds) for i in range(3)]]
def preview(path,objects,front=False):
    # Preview rig is excluded from FBX export.
    coords=[o.matrix_world@Vector(p) for o in objects if o.type=='MESH' for p in o.bound_box];center=sum(coords,Vector())/len(coords);extent=max((p-center).length for p in coords)
    bpy.ops.object.camera_add(location=center+Vector((-extent*1.7,extent*2.2,extent*.9) if front else (extent*1.7,-extent*2.2,extent*.9)));camera=bpy.context.object;camera.rotation_euler=(center-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.clip_end=max(1000,extent*10);bpy.context.scene.camera=camera
    bpy.ops.object.light_add(type='AREA',location=center+Vector((-extent,extent,extent*2) if front else (extent,-extent,extent*2)));light=bpy.context.object;light.data.energy=max(600,extent*extent*500);light.data.shape='DISK';light.data.size=max(1,extent)
    hidden=[]
    for obj in objects:
        if obj.name.startswith('Collision_') or ('LOD' in obj.name and 'LOD0' not in obj.name):obj.hide_render=True;hidden.append(obj)
    scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=12;scene.render.resolution_x=480;scene.render.resolution_y=480;scene.render.resolution_percentage=100;scene.world.color=(.18,.18,.18);scene.render.filepath=str(path);bpy.ops.render.render(write_still=True)
    for obj in hidden:obj.hide_render=False
    bpy.data.objects.remove(camera,do_unlink=True);bpy.data.objects.remove(light,do_unlink=True)
def export(name,kind,parameters,preview_image=False,metadata=None):
    folder=OUT/kind;folder.mkdir(parents=True,exist_ok=True);SOURCE.mkdir(parents=True,exist_ok=True)
    # Action authoring changes pose channels; flush evaluated matrices before FBX binds.
    bpy.context.view_layer.update()
    objects=[o for o in bpy.context.scene.objects if o.type in ('MESH','ARMATURE')];tris,bounds=validate(objects)
    blend=SOURCE/(name+'.blend');bpy.ops.wm.save_as_mainfile(filepath=str(blend))
    bpy.ops.object.select_all(action='DESELECT')
    for o in objects:o.select_set(True)
    fbx=folder/(name+'.fbx');bpy.ops.export_scene.fbx(filepath=str(fbx),use_selection=True,axis_forward='-Z',axis_up='Y',apply_scale_options='FBX_SCALE_ALL',add_leaf_bones=False,bake_anim=parameters.get('exportAnimations',False),bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,use_mesh_modifiers=True)
    image=folder/(name+'.png')
    if preview_image:preview(image,objects,parameters.get('frontPreview',False))
    record={'name':name,'type':kind,'generator':'Tools/Blender/build_all_assets.py','parameters':parameters,'blenderVersion':bpy.app.version_string,'fbxPath':str(fbx.relative_to(ROOT)),'previewPath':str(image.relative_to(ROOT)) if preview_image else '', 'triangleCount':tris,'bounds':bounds,'lodCount':parameters.get('lodCount',1),'collisionAsset':str(fbx.relative_to(ROOT)) if kind not in ('Character','Skis','EnvironmentDistant','EnvironmentAccent') else '', 'timestamp':datetime.datetime.now(datetime.timezone.utc).isoformat()}
    if metadata:(folder/(name+'.json')).write_text(json.dumps(metadata,indent=2));record['metadata']=metadata
    return record
