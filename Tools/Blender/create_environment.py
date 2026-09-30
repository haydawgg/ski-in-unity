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
    clear();steel=material('steel',(.028,.035,.05),.3);length=12;w=.13
    if kind=='kink':points=[(0,1.2,0),(0,.3,6),(0,-1.4,12)]
    elif kind=='rainbow':points=[(0,1.2+1.8*math.sin(math.pi*i/24),length*i/24) for i in range(25)]
    elif kind=='down':points=[(0,1.2,0),(0,-1.5,length)]
    else:points=[(0,1.2,0),(0,1.2,length)]
    for i in range(1,len(points)):rod('BlackTube_'+str(i),points[i-1],points[i],.07 if kind!='wide' else .13,steel,12)
    for z in [1,4,8,11]:
        y=1.2 if kind=='flat' else 1.2-.225*z
        rod('Support'+str(z),(0,-.23*z,z),(0,y,z),.04,steel,8);box('Foot'+str(z),(0,-.23*z,z),(.6,.08,.4),steel)
    metadata={'railId':kind.title(),'width':w,'points':[{'x':x,'y':y,'z':z} for x,y,z in points]}
    return export('Rail_'+kind,'Rails',{'length':length,'height':1.2,'tubeRadius':.07,'seed':42},kind=='kink',metadata)
def grind_box(kind):
    clear();steel=material('steel',(.035,.04,.055));top=material('box_top',(.20,.34,.39),.35);length=10;width=1.8 if kind=='wide' else .65;points=[]
    for i in range(21):
        z=length*i/20;y=.8-(.23*z if kind=='down' else max(0,z-5)*.3 if kind=='kink' else 0);points.append((0,y,z))
    vertices=[]
    for x,y,z in points:vertices.extend([(-width*.5,y,z),(width*.5,y,z),(-width*.5,y-.5,z),(width*.5,y-.5,z)])
    faces=[]
    for i in range(20):a=i*4;faces.extend([(a,a+4,a+5,a+1),(a,a+2,a+6,a+4),(a+1,a+5,a+7,a+3)])
    mesh('BoxRidingSurface',vertices,faces,top)
    return export('Box_'+kind,'Boxes',{'width':width,'length':length},kind=='wide',{'railId':kind.title()+' Box','width':width,'points':[{'x':x,'y':y,'z':z} for x,y,z in points]})
def jump(name,width,length,height,deck=0):
    clear();mat=snow_material();vertices=[];faces=[];sections=96;total=length+deck+(length*1.5 if deck else 0)
    for i in range(sections+1):
        z=total*i/sections
        if z<=length:y=-.23*z+height*(z/length)**2
        elif z<=length+deck:y=-.23*z+height
        else:
            t=(z-length-deck)/(total-length-deck);y=-.23*z+height*(1-t*t*(3-2*t))
        for x in [-width*.5,width*.5]:vertices.append((x,y,z))
    for i in range(sections):a=i*2;faces.append((a,a+2,a+3,a+1))
    mesh('ContinuousSnowSurface',vertices,faces,mat)
    # Sides are closed down to the terrain; no staircase riding collision.
    sideverts=[];sidefaces=[]
    for side in [0,1]:
        start=len(sideverts)
        for i in range(sections+1):x,y,z=vertices[i*2+side];sideverts.extend([(x,y,z),(x,-.23*z-.1,z)])
        for i in range(sections):a=start+i*2;sidefaces.append((a,a+1,a+3,a+2))
    mesh('SnowSides',sideverts,sidefaces,mat)
    return export(name,'Jumps',{'width':width,'length':length,'height':height,'deck':deck,'continuousCollision':True},name=='JumpMedium')
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
    clear();rng=random.Random(440+variant);mat=material('distant_rock',(.22,.25,.32));snow=snow_material();verts=[];faces=[];nx=120;nz=32
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
            y=base+profile*max(main,far,shoulder)*edge_fade
            # Small ledges and gullies break large planar faces without changing riding terrain.
            y+=math.sin(x*.041+z*.021+variant)*9*main
            verts.append((x,y,z))
    for j in range(nz):
        for i in range(nx):a=j*(nx+1)+i;faces.append((a,a+nx+1,a+nx+2,a+1))
    o=mesh('LayeredAlpineRidge',verts,faces,mat);o.data.materials.append(snow)
    for p in o.data.polygons:
        p.use_smooth=False
        if p.normal.z>.59 and p.center.z>85:p.material_index=1
    return export(f'Ridge{variant}','EnvironmentDistant',{'seed':440+variant,'layeredProfiles':True,'peakCount':len(peaks),'buriedBase':True},variant==2)

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
    clear();steel=material('steel',(.04,.045,.065));wood=material('wood',(.26,.14,.09));snow=snow_material();light=material('lamp',(.98,.87,.65))
    if kind=='Hut':
        box('RidgeLodge',(0,1.5,0),(7,3,5),wood,.05);box('SnowRoof',(0,3.1,0),(8,.28,6),snow,.05);box('Door',(0,1,.0+2.52),(1.3,2,.04),steel)
        for x in [-2,2]:box('Window'+str(x),(x,1.8,2.52),(1.1,.9,.04),light)
    elif kind=='LiftTower':
        rod('Tower',(0,0,0),(0,14,0),.24,steel);rod('CrossArm',(-4,14,0),(4,14,0),.2,steel)
        for x in [-3,3]:box('CableWheel'+str(x),(x,14.2,0),(.5,.4,.5),steel,.08)
    elif kind=='Floodlight':
        rod('Mast',(0,0,0),(0,10,0),.1,steel);box('LampHousing',(0,10,0),(1.2,.4,.5),steel,.04);box('LampLens',(0,9.95,.26),(1,.25,.04),light)
    elif kind=='Fence':
        for z in [0,2,4]:rod('Post'+str(z),(0,0,z),(0,1.2,z),.06,wood)
        for y in [.4,1]:rod('CrossRail'+str(y),(0,y,0),(0,y,4),.035,wood)
    elif kind=='Sign':
        rod('Signpost',(0,0,0),(0,2.3,0),.07,wood);box('DirectionBoard',(0,2,0),(2.5,.6,.14),wood,.06)
    elif kind=='Flag':
        rod('FlagPole',(0,0,0),(0,2.5,0),.035,steel);box('Flag',(0.5,2.1,0),(1,.55,.025),material('flag',(.93,.35,.22)))
    elif kind=='LiftCable':rod('Cable',(0,0,0),(0,0,100),.025,steel,6)
    return export(kind,'Environment',{'seed':42},kind=='Hut')
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
    for name in ['Hut','LiftTower','Floodlight','Fence','Sign','Flag','LiftCable']:records.append(prop(name))
    return records
if __name__=='__main__':build_environment()
