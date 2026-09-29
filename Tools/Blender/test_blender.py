"""Required headless smoke test: mesh, material, camera, light, PNG, blend, FBX."""
import tempfile
from pathlib import Path
import bpy
from mathutils import Vector

bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
bpy.ops.mesh.primitive_cube_add(size=1)
cube = bpy.context.object
cube.name = 'SmokeTestMesh'
mat = bpy.data.materials.new('SmokeTestMaterial')
mat.diffuse_color = (.18, .52, .65, 1)
cube.data.materials.append(mat)
bpy.ops.object.camera_add(location=(3, -4, 2))
camera = bpy.context.object
camera.rotation_euler = (-camera.location).to_track_quat('-Z', 'Y').to_euler()
bpy.context.scene.camera = camera
bpy.ops.object.light_add(type='AREA', location=(1, -2, 4))
bpy.context.object.data.energy = 500
scene = bpy.context.scene
scene.render.engine = 'CYCLES'
scene.cycles.samples = 8
scene.render.resolution_x = scene.render.resolution_y = 128
scene.render.resolution_percentage = 100
with tempfile.TemporaryDirectory(prefix='powderflow-blender-') as tmp:
    path = Path(tmp)
    bpy.ops.wm.save_as_mainfile(filepath=str(path / 'smoke.blend'))
    bpy.ops.export_scene.fbx(filepath=str(path / 'smoke.fbx'), axis_forward='-Z', axis_up='Y', use_mesh_modifiers=True, add_leaf_bones=False)
    scene.render.filepath = str(path / 'preview.png')
    bpy.ops.render.render(write_still=True)
    for name in ('smoke.blend', 'smoke.fbx', 'preview.png'):
        assert (path/name).stat().st_size > 0
        print('VALIDATED', name, (path/name).stat().st_size)
print('BLENDER SMOKE PASS', bpy.app.version_string, 'vertices', len(cube.data.vertices))
