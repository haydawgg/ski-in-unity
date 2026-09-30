"""Original park surfaces, alpine hardware and mesh-lettered wayfinding.

Collision_ meshes retain the riding geometry; the remaining meshes are visual.
Inputs use Unity meters. Rail metadata stays compatible with the original paths.
"""
from common.pipeline import *

FONT_PATH=Path('/usr/share/fonts/TTF/DejaVuSans-Bold.ttf')

def palette():
    return {name:material(name,color,rough) for name,color,rough in [
        ('steel',(.035,.055,.075),.32),('galvanized',(.43,.52,.58),.27),
        ('park_trim',(.16,.38,.40),.55),('park_accent',(.92,.34,.18),.6),
        ('sign_ink',(.035,.085,.11),.75),('sign_letter',(.93,.91,.80),.8),
        ('box_top',(.22,.42,.44),.35),('wood',(.27,.17,.115),.75),
        ('roof',(.13,.22,.25),.6),('window',(.11,.26,.32),.22),
        ('lamp',(.98,.87,.65),.4),('easy_marker',(.16,.53,.34),.7),
        ('medium_marker',(.10,.37,.72),.65),('snow',(.81,.88,.96),.65)]}

def duplicate(o,name):
    copy=o.copy();copy.data=o.data.copy();copy.name=name;bpy.context.collection.objects.link(copy);return copy

def finish(name,kind,parameters,preview=False,metadata=None):
    # Keep decorative hardware cheap to submit, with explicit collision separate.
    parts=[o for o in bpy.context.scene.objects if o.type=='MESH' and not o.name.startswith('Collision_')]
    bpy.ops.object.select_all(action='DESELECT')
    for o in parts:o.select_set(True)
    if parts:
        bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();parts[0].name='Visual_'+name
    parameters['explicitCollision']=any(o.name.startswith('Collision_') for o in bpy.context.scene.objects)
    parameters['frontPreview']=True
    if kind=='EnvironmentAccent':parameters['collision']=False
    return export(name,kind,parameters,preview,metadata)

def lettering(body,pos,size,max_width,mat):
    bpy.ops.object.text_add(location=v(pos));o=bpy.context.object;o.name='Lettering_'+body
    o.data.body=body;o.data.align_x='CENTER';o.data.align_y='CENTER';o.data.size=size;o.data.extrude=.0015;o.data.resolution_u=2
    if FONT_PATH.exists():o.data.font=bpy.data.fonts.load(str(FONT_PATH),check_existing=True)
    o.rotation_euler=(math.pi/2,0,math.pi);o.data.materials.append(mat);bpy.context.view_layer.update()
    if o.dimensions.x>max_width:o.scale*=max_width/o.dimensions.x
    bpy.ops.object.convert(target='MESH');bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    return o

def symbol(level,x,y,z,size,p):
    mat=p['easy_marker'] if level==0 else p['medium_marker'] if level==1 else p['park_accent']
    if level==0:
        rod('GreenCircle',(x,y,z-.012),(x,y,z+.012),size*.5,mat,24)
    elif level==1:box('BlueSquare',(x,y,z),(size,size,.024),mat,.025)
    else:
        o=box('OrangeDiamond',(x,y,z),(size*.74,size*.74,.024),mat,.015);o.rotation_euler[1]=math.pi/4

def path_sample(points,z):
    for a,b in zip(points,points[1:]):
        if z<=b[2]:
            t=(z-a[2])/(b[2]-a[2]);return Vector(a).lerp(Vector(b),t)
    return Vector(points[-1])

def rail(kind):
    clear();p=palette();length=12;radius=.13 if kind=='wide' else .07
    if kind=='kink':points=[(0,1.2,0),(0,.3,6),(0,-1.4,12)]
    elif kind=='rainbow':points=[(0,1.2+1.8*math.sin(math.pi*i/24),length*i/24) for i in range(25)]
    elif kind=='down':points=[(0,1.2,0),(0,-1.5,length)]
    else:points=[(0,1.2,0),(0,1.2,length)]
    for i in range(1,len(points)):
        tube=rod('Tube'+str(i),points[i-1],points[i],radius,p['steel'],12);duplicate(tube,'Collision_RailSpan'+str(i))
    for z in [1,4,8,11]:
        center=path_sample(points,z);base=Vector((0,-.23*z,z));top=center-Vector((0,radius*.6,0))
        rod('Upright',base,top,.065,p['steel'],10)
        rod('Saddle',(-.16,top.y,z),(.16,top.y,z),.045,p['galvanized'],8)
        box('Footplate',(0,base.y,z),(.85,.09,.55),p['galvanized'],.018)
        for x in [-.30,.30]:box('AnchorBolt',(x,base.y+.065,z),(.065,.04,.065),p['steel'],.007)
        rod('Diagonal',(0,base.y,z+.55),(0,top.y-.08,z),.035,p['steel'],8)
    # Colored entry/exit sleeves match the collision radius, with millimeter relief.
    for z in [.16,length-.34]:
        a=path_sample(points,z);b=path_sample(points,z+.17)
        rod('EntrySleeve',a,b,radius+.002,p['park_accent'] if z<1 else p['park_trim'],12)
    for z in [1.8,5.1,9.0]:
        a=path_sample(points,z)+Vector((.025,radius+.001,0));b=path_sample(points,z+.35)+Vector((.02,radius+.001,0))
        rod('RestrainedWear',a,b,.0025,p['galvanized'],5)
    metadata={'railId':kind.title(),'width':.13,'points':[{'x':x,'y':y,'z':z} for x,y,z in points]}
    return finish('Rail_'+kind,'Rails',{'length':length,'height':1.2,'tubeRadius':radius,'seed':42,'connectedSupports':True},kind=='kink',metadata)

def grind_box(kind):
    clear();p=palette();length=10;width=1.8 if kind=='wide' else .65
    points=[(0,.8-(.23*(length*i/20) if kind=='down' else max(0,length*i/20-5)*.3 if kind=='kink' else 0),length*i/20) for i in range(21)]
    vertices=[];faces=[]
    for x,y,z in points:vertices.extend([(-width*.5,y,z),(width*.5,y,z),(-width*.5,y-.5,z),(width*.5,y-.5,z)])
    for i in range(20):
        a=i*4;faces.extend([(a,a+4,a+5,a+1),(a,a+2,a+6,a+4),(a+1,a+5,a+7,a+3)])
    riding=mesh('RidingSurface',vertices,faces,p['box_top']);duplicate(riding,'Collision_BoxRidingSurface')
    # End panels and underside close the previous open shell, below the riding top.
    mesh('ClosedShell',vertices,[(0,1,3,2),(80,82,83,81)]+[(i*4+2,i*4+3,i*4+7,i*4+6) for i in range(20)],p['steel'])
    for i in range(20):
        a,b=points[i],points[i+1]
        for side in [-1,1]:
            rod('BeveledEdge',(side*(width*.5-.018),a[1]-.025,a[2]),(side*(width*.5-.018),b[1]-.025,b[2]),.024,p['galvanized'],8)
            rod('LowerTrim',(side*(width*.5+.004),a[1]-.42,a[2]),(side*(width*.5+.004),b[1]-.42,b[2]),.03,p['park_trim'],6)
    for z in [1,4,7,9]:
        top=path_sample(points,z).y-.5;ground=-.23*z
        for x in [-width*.32,width*.32]:
            rod('BoxLeg',(x,ground,z),(x,top+.04,z),.055,p['steel'],8)
            box('BoxFoot',(x,ground,z),(.35,.08,.42),p['galvanized'],.018)
    for z in [0,length]:
        y=path_sample(points,z).y-.16
        box('EndBand',(0,y,z+(-.018 if z==0 else .018)),(width*.82,.15,.03),p['park_accent'] if z<1 else p['park_trim'],.01)
    for z in [2.4,5.7,7.9]:
        center=path_sample(points,z);rod('TopWear',(.08,center.y+.001,z),(.05,path_sample(points,z+.5).y+.001,z+.5),.0025,p['galvanized'],5)
    return finish('Box_'+kind,'Boxes',{'width':width,'length':length,'closedEnds':True},kind=='wide',{'railId':kind.title()+' Box','width':width,'points':[{'x':x,'y':y,'z':z} for x,y,z in points]})

def jump(name,width,length,height,deck=0):
    clear();p=palette();vertices=[];faces=[];sections=96;total=length+deck+(length*1.5 if deck else 0)
    for i in range(sections+1):
        z=total*i/sections
        if z<=length:y=-.23*z+height*(z/length)**2
        elif z<=length+deck:y=-.23*z+height
        else:
            t=(z-length-deck)/(total-length-deck);y=-.23*z+height*(1-t*t*(3-2*t))
        for x in [-width*.5,width*.5]:vertices.append((x,y,z))
    for i in range(sections):a=i*2;faces.append((a,a+2,a+3,a+1))
    surface=mesh('ContinuousSnowSurface',vertices,faces,p['snow'])
    # An open curved strip has no enclosed "outside" for normal recalculation.
    # Explicitly face the riding side upward so Unity casts hit the ramp, not terrain below.
    for polygon in surface.data.polygons:
        if polygon.normal.z<0:polygon.flip()
    surface.data.update();assert all(polygon.normal.z>0 for polygon in surface.data.polygons)
    duplicate(surface,'Collision_ContinuousSnowSurface')
    # Original sides remain the collision boundary; flared sculpted snow is visual.
    sideverts=[];sidefaces=[];sculpt=[];sculptfaces=[]
    for side in [0,1]:
        start=len(sideverts);start_s=len(sculpt);direction=-1 if side==0 else 1
        for i in range(sections+1):
            x,y,z=vertices[i*2+side];base=-.23*z-.1;sideverts.extend([(x,y,z),(x,base,z)])
            spread=.25+min(1.8,max(0,y-base)*.38)
            # Keep raised snow inside the body's clearance from the existing side collider.
            # The broad toe remains low on the grade, rather than burying riders beside a jump.
            inner=min(.20,spread*.35);shoulder=base+min(.08,max(0,y-base)*.2)
            sculpt.extend([(x,y,z),(x+direction*inner,shoulder,z),(x+direction*spread,base,z)])
        for i in range(sections):
            a=start+i*2;sidefaces.append((a,a+1,a+3,a+2))
            a=start_s+i*3;sculptfaces.extend([(a,a+1,a+4,a+3),(a+1,a+2,a+5,a+4)])
    mesh('Collision_SnowSides',sideverts,sidefaces,p['snow']);mesh('SculptedSnowBanks',sculpt,sculptfaces,p['snow'])
    # Packed apron follows the existing grade; it introduces no additional collider.
    apron=[(-width*.5-.25,.23*4+.012,-4),(width*.5+.25,.23*4+.012,-4),(-width*.5,.012,0),(width*.5,.012,0)]
    mesh('PackedApproach',apron,[(0,2,3,1)],p['snow'])
    return finish(name,'Jumps',{'width':width,'length':length,'height':height,'deck':deck,'continuousCollision':True,'topFacingUp':True,'sculptedSides':True,'raisedBankClearance':.20},name=='JumpMedium')

REGIONS=[('Easy','RIDGE / EASY','FLOW LINE 01',0),('Park','TERRAIN PARK','PARK LINE 02',1),('BigAir','BIG AIR','EXPERT LINE 03',2),('Freeride','FREERIDE','TREE GAPS 04',2),('Lower','LOWER RUN','HOMEWARD 05',0)]

def sign(kind,p):
    data=next((r for r in REGIONS if kind=='Sign'+r[0]),('','POWDERFLOW','FIND YOUR LINE',1))
    _,title,subtitle,level=data
    for x in [-1.23,1.23]:
        rod('BoardPost',(x,0,0),(x,3.1,0),.075,p['wood'],8)
        box('PostCap',(x,3.13,0),(.22,.09,.22),p['park_trim'],.02)
        box('Collision_Post'+str(x),(x,1.5,0),(.15,3,.15),p['wood'])
    panel=box('SignBoard',(0,2.6,0),(3.35,1.25,.20),p['sign_ink'],.045);duplicate(panel,'Collision_SignBoard')
    box('SignTopTrim',(0,3.22,0),(3.4,.08,.24),p['park_trim'],.015)
    box('BoardSnow',(0,3.28,0),(3.37,.09,.26),p['snow'],.035)
    lettering(title,(.22,2.85,-.112),.29,2.45,p['sign_letter'])
    lettering(subtitle,(.22,2.47,-.112),.14,2.45,p['sign_letter'])
    lettering('PF',(1.40,2.16,-.112),.13,.4,p['park_accent'])
    symbol(level,-1.23,2.76,-.125,.38,p)
    # Descending chevron is shared across region boards and feature markers.
    arrow=mesh('DownArrow',[(-1.40,2.39,-.12),(-1.23,2.22,-.12),(-1.06,2.39,-.12),(-1.12,2.45,-.12),(-1.23,2.34,-.12),(-1.34,2.45,-.12)],[(0,1,2,3,4,5)],p['sign_letter'])
    mod=arrow.modifiers.new('Raised arrow','SOLIDIFY');mod.thickness=.018
    bpy.context.view_layer.objects.active=arrow;bpy.ops.object.modifier_apply(modifier=mod.name)

def prop(kind):
    clear();p=palette();accent=kind in ['LiftChair','StartGate','BoundaryMarker','FeatureEasy','FeatureMedium','FeatureExpert','LiftCable','Flag']
    if kind=='Hut':
        body=box('LodgeBody',(0,1.5,0),(7,3,5),p['wood'],.04);duplicate(body,'Collision_LodgeBody')
        for z in [-2.52,2.52]:
            for y in [.3,.6,.9,1.2,1.5,1.8,2.1,2.4,2.7]:box('CladdingJoint',(0,y,z),(6.9,.025,.025),p['roof'])
        roofverts=[(-4,2.98,-3),(4,2.98,-3),(-4,2.98,3),(4,2.98,3),(0,4.1,-3),(0,4.1,3)]
        mesh('GabledRoof',roofverts,[(0,4,5,2),(4,1,3,5)],p['roof'])
        mesh('RoofSnow',[(x,y+.08,z) for x,y,z in roofverts],[(0,4,5,2),(4,1,3,5)],p['snow'])
        gableverts=[(x,y,z) for z in [-2.52,2.52] for x,y in [(-3.5,2.98),(3.5,2.98),(3.5,3.12),(0,4.1),(-3.5,3.12)]]
        gables=mesh('ClosedGables',gableverts,[(0,1,2,3,4),(5,9,8,7,6)],p['wood'])
        mod=gables.modifiers.new('Solid gable walls','SOLIDIFY');mod.thickness=.08
        bpy.context.view_layer.objects.active=gables;bpy.ops.object.modifier_apply(modifier=mod.name)
        for z in [-3,3]:
            rod('Fascia',(-4,2.98,z),(0,4.1,z),.10,p['park_trim'],8);rod('Fascia',(0,4.1,z),(4,2.98,z),.10,p['park_trim'],8)
        box('Door',(0,1.1,-2.54),(1.3,2.2,.08),p['roof'],.025);box('DoorInset',(0,1.62,-2.59),(.85,.7,.035),p['window'],.01)
        box('DoorHandle',(.43,1.0,-2.61),(.055,.30,.055),p['galvanized'],.01)
        for x in [-2.25,2.25]:
            box('WindowFrame',(x,1.75,-2.54),(1.35,1.12,.12),p['sign_letter'],.025)
            box('WindowPane',(x,1.75,-2.615),(1.13,.9,.02),p['window'])
            for dx in [-.58,0,.58]:box('WindowMullion',(x+dx,1.75,-2.635),(.045,.95,.035),p['roof'])
            box('WindowSill',(x,1.18,-2.65),(1.5,.12,.30),p['park_trim'],.02)
            box('SillSnow',(x,1.26,-2.66),(1.42,.08,.27),p['snow'],.025)
        box('LodgeName',(0,2.80,-2.56),(2.0,.34,.05),p['sign_ink'],.02);lettering('RIDGE HOUSE',(0,2.80,-2.598),.18,1.82,p['sign_letter'])
        box('Chimney',(2,3.6,1.1),(.6,1.8,.65),p['roof'],.02);box('ChimneyCap',(2,4.53,1.1),(.8,.13,.85),p['galvanized'],.02)
        box('Porch',(0,.04,-3.25),(3.4,.12,1.4),p['wood'],.025)
    elif kind=='LiftTower':
        mast=rod('Tower',(0,0,0),(0,14,0),.24,p['steel'],12);duplicate(mast,'Collision_Tower')
        box('ConcreteFoot',(0,.22,0),(1.25,.50,1.25),p['galvanized'],.08)
        rod('CrossArm',(-4,14,0),(4,14,0),.2,p['steel'],10)
        for x in [-3,3]:
            for z in [-.55,.55]:
                rod('Sheave',(x-.09,14.4,z),(x+.09,14.4,z),.27,p['galvanized'],16)
                rod('SheaveHub',(x-.12,14.4,z),(x+.12,14.4,z),.085,p['steel'],10)
            rod('WheelSupport',(x,13.9,-.7),(x,13.9,.7),.07,p['park_trim'],8)
            rod('ArmBrace',(0,11.5,0),(x,13.9,0),.065,p['steel'],8)
        for y in [i*.38+.45 for i in range(34)]:rod('LadderRung',(-.3,y,-.34),(.3,y,-.34),.022,p['galvanized'],6)
        for x in [-.34,.34]:rod('LadderRail',(x,.35,-.34),(x,13.3,-.34),.032,p['galvanized'],6)
    elif kind=='LiftChair':
        rod('Hanger',(0,0,0),(0,-2.5,0),.055,p['steel'],8)
        rod('Yoke',(-1.2,-2.5,0),(1.2,-2.5,0),.055,p['galvanized'],8)
        for x in [-1.2,1.2]:
            rod('SeatFrame',(x,-2.5,0),(x,-3.1,-.45),.045,p['steel'],8)
            rod('BackFrame',(x,-3.1,-.45),(x,-2.45,-.45),.04,p['steel'],8)
        box('Bench',(0,-3.08,0),(2.55,.16,.9),p['park_trim'],.055)
        box('Backrest',(0,-2.69,-.44),(2.55,.6,.12),p['park_trim'],.05)
        rod('SafetyBar',(-1.15,-2.8,.45),(1.15,-2.8,.45),.035,p['galvanized'],8)
    elif kind=='LiftCable':
        points=[(0,-4*(i/24)*(1-i/24),100*i/24) for i in range(25)]
        for a,b in zip(points,points[1:]):rod('CableSpan',a,b,.025,p['steel'],6)
    elif kind=='Floodlight':
        mast=rod('Mast',(0,0,0),(0,10,0),.1,p['steel'],10);duplicate(mast,'Collision_Mast')
        box('PoleFoot',(0,.15,0),(.55,.3,.55),p['galvanized'],.03)
        rod('LampArm',(-1,9.8,0),(1,9.8,0),.07,p['galvanized'],8)
        for x in [-.65,.65]:
            box('LampHousing',(x,10,0),(.65,.5,.55),p['steel'],.06)
            box('WarmLens',(x,10,-.29),(.49,.32,.025),p['lamp'],.035)
            box('RainHood',(x,10.28,-.1),(.77,.07,.75),p['park_trim'],.02)
            for y in [9.88,10.02]:box('LensGuard',(x,y,-.32),(.54,.018,.025),p['galvanized'])
    elif kind=='Fence':
        for z in [0,2,4]:
            post=rod('Post'+str(z),(0,-.23*z,z),(0,1.2-.23*z,z),.06,p['wood'],8);duplicate(post,'Collision_Post'+str(z))
            box('FenceCap',(0,1.23-.23*z,z),(.16,.10,.16),p['park_trim'],.02)
        for y in [.4,1]:rod('CrossRail'+str(y),(0,y,0),(0,y-.23*4,4),.035,p['wood'],8)
    elif kind.startswith('Sign'):sign(kind,p)
    elif kind.startswith('Feature'):
        level=['FeatureEasy','FeatureMedium','FeatureExpert'].index(kind)
        rod('MarkerPole',(0,0,0),(0,2.15,0),.05,p['steel'],8)
        box('EntryPanel',(0,1.78,0),(.62,.8,.10),p['sign_ink'],.035)
        symbol(level,0,1.96,-.065,.26,p)
        lettering(['FLOW','PARK','EXPERT'][level],(0,1.62,-.065),.085,.52,p['sign_letter'])
        lettering('ENTRY',(0,1.47,-.065),.07,.52,p['sign_letter'])
    elif kind=='BoundaryMarker':
        rod('BoundaryPole',(0,0,0),(0,1.5,0),.035,p['park_trim'],8)
        for y in [.85,1.15,1.43]:rod('ReflectiveBand',(0,y-.055,0),(0,y+.055,0),.039,p['park_accent'],8)
    elif kind=='StartGate':
        for x in [-10,10]:
            rod('GatePost',(x,0,0),(x,5.1,0),.10,p['wood'],10)
            box('GateFoot',(x,.16,0),(.55,.32,.55),p['park_trim'],.04)
        box('GateHeader',(0,4.75,0),(20.5,.75,.2),p['sign_ink'],.04)
        lettering('POWDERFLOW',(0,4.77,-.115),.55,10,p['sign_letter'])
        for x in [-8,8]:symbol(0,x,4.77,-.125,.42,p)
    elif kind=='Flag':
        rod('FlagPole',(0,0,0),(0,2.5,0),.035,p['steel'],8)
        verts=[]
        for i in range(9):
            x=i/8;wave=.10*math.sin(x*7)*x
            verts.extend([(x,2.4-.05*x,wave),(x,1.84-.05*x,wave)])
        cloth=mesh('FlagCloth',verts,[(i*2,i*2+2,i*2+3,i*2+1) for i in range(8)],p['park_accent'])
        mod=cloth.modifiers.new('Cloth thickness','SOLIDIFY');mod.thickness=.018
        bpy.context.view_layer.objects.active=cloth;bpy.ops.object.modifier_apply(modifier=mod.name)
        box('FlagStripe',(.23,2.14,-.012),(.09,.51,.022),p['sign_letter'])
    else:raise ValueError(kind)
    return finish(kind,'EnvironmentAccent' if accent else 'Environment',{'seed':42,'parkPolish':True},kind in ['Hut','SignPark','LiftTower','LiftChair'])

def build_jumps():
    return [jump(*args) for args in [('JumpSmall',6,10,1.8,0),('JumpMedium',10,16,4.5,0),('JumpLarge',18,24,8,0),('Tabletop',18,18,5,12),('Hip',12,15,4,0),('QuarterPipe',16,7,6,0)]]

def build_park():
    records=[rail(k) for k in ['flat','down','kink','rainbow','wide']]
    records.extend(grind_box(k) for k in ['flat','down','wide','narrow','kink'])
    records.extend(build_jumps())
    records.extend(prop(k) for k in ['Hut','LiftTower','Floodlight','Fence','Sign','Flag','LiftCable','LiftChair','StartGate','BoundaryMarker','FeatureEasy','FeatureMedium','FeatureExpert'])
    records.extend(prop('Sign'+r[0]) for r in REGIONS)
    return records
