"""Seeded alpine terrain, terrain park, LOD pines, rocks and mountain props."""
from common.pipeline import *

def terrain_height(x,z):
    central=math.exp(-(x/48)**4)
    edge=(abs(x)/200)**3*95
    ripple=(math.sin(z*.022+x*.023)*3+math.sin(z*.07-x*.02)*1.5)*(1-central*.82)
    freeride=math.sin(z*.046)*4*max(0,min(1,(x-50)/55))
    return 355-.23*z+edge+ripple+freeride

def snow_material():return material('snow',(.81,.88,.96))

class SceneryMesh:
    """One mesh per LOD, with closed branch/snow lobes and shared materials."""
    def __init__(self):self.vertices=[];self.faces=[];self.slots=[]
    def loft(self,stations,side,sides,slot,half=False):
        start=len(self.vertices);side=Vector(side);up=Vector((0,1,0))
        for center,width,thickness in stations:
            for i in range(sides):
                angle=math.pi*i/(sides-1) if half else 2*math.pi*i/sides
                self.vertices.append(tuple(Vector(center)+side*math.cos(angle)*width+up*math.sin(angle)*thickness))
        faces=[tuple(start+i for i in reversed(range(sides)))]
        for j in range(len(stations)-1):
            for i in range(sides):
                a=start+j*sides+i;b=start+j*sides+(i+1)%sides
                faces.append((a,b,b+sides,a+sides))
        faces.append(tuple(start+(len(stations)-1)*sides+i for i in range(sides)))
        self.faces.extend(faces);self.slots.extend([slot]*len(faces))
    def finish(self,name,materials):
        o=mesh(name,self.vertices,self.faces,materials[0])
        for m in materials[1:]:o.data.materials.append(m)
        for p,slot in zip(o.data.polygons,self.slots):p.material_index=slot
        return o

def tree(variant):
    clear();height=[5,8,12,7][variant];green=material('pine_dark',(.075,.16,.145));snow=snow_material();bark=material('bark',(.16,.115,.085))
    rng=random.Random(42+variant);tiers=[7,8,9,7][variant];branches=[]
    lean=Vector((rng.uniform(-.025,.025)*height,0,rng.uniform(-.025,.025)*height))
    for tier in range(tiers):
        t=tier/(tiers-1);y=height*(.18+t*.70);radius=height*(.235*(1-t)**.8+.022)
        count=5+(tier+variant)%3;turn=rng.uniform(0,math.tau)
        for j in range(count):
            angle=turn+j*math.tau/count+rng.uniform(-.18,.18);length=radius*rng.uniform(.73,1.20)
            branches.append((y+rng.uniform(-.022,.022)*height,angle,length,rng.uniform(.85,1.15)))
    lod_triangles=[]
    for lod,sides in enumerate([6,4,3]):
        g=SceneryMesh()
        # Vertical trunk uses horizontal ring geometry rather than the branch loft.
        trunk=rod(f'Tree_LOD{lod}_trunk',(0,0,0),(lean.x*.91,height*.91,lean.z*.91),height*.018,bark,[8,6,4][lod])
        for y,angle,length,bulk in branches:
            direction=Vector((math.cos(angle),0,math.sin(angle)));side=(-math.sin(angle),0,math.cos(angle));origin=lean*(y/height)+Vector((0,y,0))
            width=length*.25*bulk
            stations=[(tuple(origin),width*.32,width*.30),
                      (tuple(origin+direction*length*.48+Vector((0,-height*.015,0))),width,width*.38),
                      (tuple(origin+direction*length*.88+Vector((0,-height*.046,0))),width*.34,width*.16),
                      (tuple(origin+direction*length+Vector((0,-height*.062,0))),width*.035,width*.025)]
            if lod==2:stations=[stations[0],stations[1],stations[-1]]
            g.loft(stations,side,sides,0)
            loaded=[]
            for center,w,d in stations:
                loaded.append((tuple(Vector(center)+Vector((0,width*.22,0))),w*.91,d*.82))
            g.loft(loaded,side,sides,1,half=True)
        # Broken, narrow crown joins the upper whorl; no large stacked cones.
        bpy.ops.mesh.primitive_cone_add(vertices=[8,6,4][lod],radius1=height*.042,radius2=0,depth=height*.16,location=v((lean.x,height*.94,lean.z)))
        crown=bpy.context.object;crown.name=f'Tree_LOD{lod}_crown';crown.data.materials.append(green)
        o=g.finish(f'Tree_LOD{lod}',[green,snow,bark])
        bpy.ops.object.select_all(action='DESELECT')
        for part in [o,trunk,crown]:part.select_set(True)
        bpy.context.view_layer.objects.active=o;bpy.ops.object.join()
        o.data.calc_loop_triangles();lod_triangles.append(len(o.data.loop_triangles))
    return export(f'Pine{variant}','Trees',{'seed':42+variant,'height':height,'lodCount':3,'lodTriangles':lod_triangles,'asymmetricWhorls':True},variant==2)

def shrub(variant):
    clear();rng=random.Random(620+variant);green=material('pine_dark',(.075,.16,.145));snow=snow_material();bark=material('bark',(.16,.115,.085))
    stems=[(rng.uniform(0,math.tau),rng.uniform(.45,.95),rng.uniform(.35,.85)) for _ in range(6+variant*2)]
    counts=[]
    for lod,sides in enumerate([6,4,3]):
        g=SceneryMesh()
        for angle,length,height in stems:
            direction=Vector((math.cos(angle),0,math.sin(angle)));side=(-math.sin(angle),0,math.cos(angle))
            stations=[((0,.1,0),.10,.08),(tuple(direction*length*.5+Vector((0,height,0))),.21,.14),(tuple(direction*length+Vector((0,height*.8,0))),.03,.02)]
            g.loft(stations,side,sides,0)
            g.loft([(tuple(Vector(c)+Vector((0,.10,0))),w*.9,d*.8) for c,w,d in stations],side,sides,1,True)
        o=g.finish(f'Shrub_LOD{lod}',[green,snow]);o.data.calc_loop_triangles();counts.append(len(o.data.loop_triangles))
    return export(f'Shrub{variant}','EnvironmentAccent',{'seed':620+variant,'lodCount':3,'lodTriangles':counts,'collision':False},variant==0)
def rock(variant):
    clear();rng=random.Random(420+variant);stone=material('rock',(.19,.22,.28));snow=snow_material();o=ellipsoid('Rock',(.0,.55,0),(1.25+variant*.14,.75+variant*.10,1.05+variant*.07),stone,10,6);o.data.materials.append(snow)
    for vert in o.data.vertices:vert.co*=rng.uniform(.78,1.19)
    o.data.update()
    for face in o.data.polygons:
        face.use_smooth=False
        if face.normal.z>.48 and face.center.z>0:face.material_index=1
    return export(f'Rock{variant}','Rocks',{'seed':420+variant,'snowLedges':True},variant==0)
def rail(kind):
    from create_park import rail as generate
    return generate(kind)
def grind_box(kind):
    from create_park import grind_box as generate
    return generate(kind)
def jump(name,width,length,height,deck=0):
    from create_park import jump as generate
    return generate(name,width,length,height,deck)
def mountain(chunk):
    clear();mat=snow_material();vertices=[];faces=[];nx=80;nz=30
    for j in range(nz+1):
        z=chunk*150+j*5
        for i in range(nx+1):x=-200+i*5;vertices.append((x,terrain_height(x,z),z))
    for j in range(nz):
        for i in range(nx):a=j*(nx+1)+i;faces.append((a,a+nx+1,a+nx+2,a+1))
    o=mesh('SmoothMountainChunk',vertices,faces,mat)
    for p in o.data.polygons:p.use_smooth=True
    return export(f'Mountain{chunk:02}','Mountain',{'chunk':chunk,'length':150,'width':400,'spacing':5,'seed':42})
def ridge(variant):
    clear();rng=random.Random(440+variant);mat=material('distant_rock',(.22,.25,.32));snow=snow_material();verts=[];faces=[];nx=144;nz=48
    peaks=[];x=-780
    while x<850:
        x+=rng.uniform(120,240);peaks.append((x,rng.uniform(180,360),rng.uniform(70,155),rng.uniform(-.35,.35)))
    for j in range(nz+1):
        z=720*j/nz
        for i in range(nx+1):
            x=-850+1700*i/nx;profile=90
            for center,height,width,skew in peaks:
                delta=(x-center)/width;profile=max(profile,100+height*math.exp(-abs(delta*(1+skew*(1 if delta>0 else -1)))**1.4))
            ridge_z=280+42*math.sin(x*.005+variant)+24*math.sin(x*.014-variant)
            main=math.exp(-abs((z-ridge_z)/140)**1.7)
            shoulder=math.exp(-((z-115-20*math.sin(x*.009))/95)**2)*.26
            far=math.exp(-abs((z-545-30*math.sin(x*.007+2))/115)**1.8)*.62
            edge_fade=max(0,min(1,(850-abs(x))/140));base=-65+14*math.sin(x*.009+variant)+8*math.sin(z*.017+x*.006)
            # Branching spurs, shelves and gullies give nearby faces a craggy silhouette.
            relief=(10*math.sin(x*.041+z*.021+variant)+7*math.sin(x*.077-z*.049)+5*math.sin(z*.089+x*.022))*main
            y=base+(profile*max(main,far,shoulder)+relief)*edge_fade
            verts.append((x,y,z))
    for j in range(nz):
        for i in range(nx):a=j*(nx+1)+i;faces.append((a,a+nx+1,a+nx+2,a+1))
    o=mesh('LayeredAlpineRidge',verts,faces,mat);o.data.materials.append(snow)
    # Open meshes have no volume from which recalc can infer an outside. Keep +Z up.
    for polygon in o.data.polygons:
        if polygon.normal.z<0:polygon.flip()
    o.data.update()
    for p in o.data.polygons:
        p.use_smooth=False
        gully=math.sin(p.center.x*.025+p.center.y*.017+variant)+.5*math.sin(p.center.x*.058-p.center.y*.031)
        if p.center.z>45 and (p.normal.z>.50 or (p.normal.z>.22 and gully>.1)):p.material_index=1
    return export(f'Ridge{variant}','EnvironmentDistant',{'seed':440+variant,'layeredProfiles':True,'peakCount':len(peaks),'buriedBase':True,'cragDetail':True,'upwardNormals':True,'snowShelvesAndGullies':True},variant==2)

def valley():
    clear();vertices=[];faces=[];nx=60;nz=32
    for j in range(nz+1):
        z=1500+j*40;t=min(1,j/9);blend=t*t*(3-2*t)
        for i in range(nx+1):
            x=-900+i*30
            start=terrain_height(max(-200,min(200,x)),1500)+max(0,abs(x)-200)*.035
            floor=2-.008*(z-1500)+54*(abs(x)/900)**1.7+math.sin(x*.01+z*.008)*3
            vertices.append((x,start*(1-blend)+floor*blend,z))
    for j in range(nz):
        for i in range(nx):a=j*(nx+1)+i;faces.append((a,a+nx+1,a+nx+2,a+1))
    o=mesh('RollingLowerValley',vertices,faces,snow_material())
    for p in o.data.polygons:p.use_smooth=True
    return export('ValleyFloor','EnvironmentDistant',{'seed':42,'collision':False,'buriedRidgeTransition':True})

def build_scenery():
    records=[tree(i) for i in range(4)]+[rock(i) for i in range(6)]+[shrub(i) for i in range(2)]
    records.extend(ridge(i) for i in range(5));records.append(valley());return records
def prop(kind):
    from create_park import prop as generate
    return generate(kind)
def build_environment():
    records=[]
    for i in range(4):records.append(tree(i))
    for i in range(6):records.append(rock(i))
    for kind in ['flat','down','kink','rainbow','wide']:records.append(rail(kind))
    for kind in ['flat','down','wide','narrow','kink']:records.append(grind_box(kind))
    for args in [('JumpSmall',6,10,1.8,0),('JumpMedium',10,16,4.5,0),('JumpLarge',18,24,8,0),('Tabletop',18,18,5,12),('Hip',12,15,4,0),('QuarterPipe',16,7,6,0)]:records.append(jump(*args))
    for i in range(10):records.append(mountain(i))
    for i in range(2):records.append(shrub(i))
    for i in range(5):records.append(ridge(i))
    records.append(valley())
    from create_park import REGIONS
    for name in ['Hut','LiftTower','Floodlight','Fence','Sign','Flag','LiftCable','LiftChair','StartGate','BoundaryMarker','FeatureEasy','FeatureMedium','FeatureExpert']+['Sign'+r[0] for r in REGIONS]:records.append(prop(name))
    return records
if __name__=='__main__':build_environment()
