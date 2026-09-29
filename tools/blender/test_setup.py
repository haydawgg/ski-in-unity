"""Headless Blender smoke test; outputs are intentionally temporary."""
import os
import tempfile
from pathlib import Path

import bpy
from mathutils import Vector

print("Blender version:", bpy.app.version_string)
bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)
bpy.ops.mesh.primitive_cube_add()
cube = bpy.context.object
cube.name = "SetupTestCube"
material = bpy.data.materials.new("SetupTestMaterial")
material.diffuse_color = (0.1, 0.5, 0.9, 1.0)
cube.data.materials.append(material)
bpy.ops.object.camera_add(location=(4, -6, 3))
camera = bpy.context.object
direction = Vector((0, 0, 0)) - camera.location
camera.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()
bpy.context.scene.camera = camera
bpy.ops.object.light_add(type="AREA", location=(1, -3, 5))
bpy.context.object.data.energy = 500

with tempfile.TemporaryDirectory(prefix="powderline-blender-") as tmp:
    folder = Path(tmp)
    bpy.ops.wm.save_as_mainfile(filepath=str(folder / "test.blend"))
    bpy.ops.export_scene.gltf(filepath=str(folder / "test.glb"), export_format="GLB")
    bpy.context.scene.render.engine = "CYCLES"
    bpy.context.scene.cycles.samples = 4
    bpy.context.scene.render.resolution_x = 64
    bpy.context.scene.render.resolution_y = 64
    bpy.context.scene.render.resolution_percentage = 100
    bpy.context.scene.render.filepath = str(folder / "preview.png")
    bpy.ops.render.render(write_still=True)
    for name in ("test.blend", "test.glb", "preview.png"):
        path = folder / name
        assert path.is_file() and path.stat().st_size > 0, name
        print(f"Verified {name}: {path.stat().st_size} bytes")
print("Blender CLI setup passed")
