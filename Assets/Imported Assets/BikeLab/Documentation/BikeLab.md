# Bike Lab

The basis of the Bike Lab package is the BikeController script. BikeController implements the physics of the bike and allows you to control it manually or using another program. This program, for example, can drive the bike to a target object (Motobol) or along a track.

Track is the second important part of the Bike Lab package. You can create tracks for motocross, road racing, mountain tracks, city tracks, etc.

The next necessary part is the motorcyclist. The humanoid model is controlled using IK.

The Bike Lab package also includes a Segway Controller. This is a relatively simple script since the balance task for a Segway is much simpler than for a Bike.

The sections in this guide correspond to project folders.

**Table of Contents**
- 1 [Bike](#bike)
    - 1.1 [Physics](#physics)
        - 1.1.1 [Bike](#bikescript)
        - 1.1.2 [Feet](#feet)
    - 1.2 [Input](#input)
        - 1.2.1 [ManualControl](#manualcontrol)
        - 1.2.2 [BikeInput](#bikeinput)
        - 1.2.3 [DeviceDropdown](#devicedropdown)
    - 1.3 [Bike parts](#parts)
        - 1.3.1 [Fork](#fork)
        - 1.3.2 [Swingarm](#swingarm)
        - 1.3.3 [Wheel](#wheel)
        - 1.3.4 [Damper](#damper)
        - 1.3.5 [Chain](#chain)
        - 1.3.6 [Pedals](#pedals)
    - 1.4 [Other Bike scripts](#other)
        - 1.4.1 [BikeTrackController](#biketrackcontroller)
        - 1.4.2 [SlipEffects](#slipeffects)
        - 1.4.3 [Sound](#sound)
        - 1.4.4 [WheelColliderInterpolator](#wheelColliderInterpolator)
- 2 [Biker](#biker)
    - 2.1 [IKcontrol](#ikcontrol)
    - 2.2 [FootContact](#footcontact)
- 3 [Track](#track)
    - 3.1 [Spline](#spline)
        - 3.1.1 [SplineSegment](#splinesegment)
        - 3.1.2 [SplineBase](#splinebase)
        - 3.1.3 [Spline1](#spline1)
        - 3.1.4 [Spline2](#spline2)
    - 3.2 [Track Spline](#track-spline)
        - 3.2.1 [TrackSpline Script](#tracksplinescript)
        - 3.2.2 [TrackSpline2](#trackspline2)
    - 3.3 [TrackTerrain](#trackterrain)
    - 3.4 [TrackMesh](#trackMesh)
    - 3.5 [TrackController](#trackcontroller)
    - 3.6 [TrackDispatcher](#trackdispatcher)
    - 3.7 [IVehicle](#ivehicle)
    - 3.8 [Actions](#actions)
    - 3.9 [TrafficLights](#trafficlights)
        - 3.9.1 [TrafficLight](#trafficlight)
        - 3.9.2 [TrafficLightBox](#trafficlightbox)
    - 3.10 [SpeedLimits](#speedlimits)
- 4 [Motoball](#motoball)
    - 4.1 [MotoballController](#motoballcontroller)
- 5 [Segway](#segway)
    - 5.1 [Segway](#segwayscript)
    - 5.2 [SegwayTrackController](#segwaytrackcontroller)
    - 5.3 [SegwayIK](#segwayik)
- 6 [Examples](#examples)
    - 6.1 [Bicycle](#bicycle)
    - 6.2 [Bike](#bikeexample)
    - 6.3 [Demo](#demo)
    - 6.4 [Motocross](#motocross)
        - 6.4.1 [BigJump](#bigjump)
        - 6.4.2 [Motocross](#motocrossscene)
        - 6.4.3 [Slow](#slow)
    - 6.5 [RoadRacing](#roadracing)
        - 6.5.1 [Donington](#donington)
        - 6.5.2 [Eight](#eight)
        - 6.5.3 [Suzuka](#suzuka)
    - 6.6 [Segway](#segway)
        - 6.6.1 [Segway](#segwayscene)
        - 6.6.2 [Sity](#sity)
        - 6.6.3 [Track](#track)

<a name="bike"></a>
## 1 Bike

<a name="physics"></a>
### 1.1 Physics

<a name="bikescript"></a>
#### 1.1.1 Bike
**Description**<br>
Bike script allows you to control a bike, which consists of two WheelColliders and a RigidBody. BikeController itself does not use user input. There are public methods to control the bike: setSteer, SetAcceleration etc. It follows from this that another script is needed to control the bike. This script can use user input, for example.

For a given speed and lean angle, there is a steering angle that ensures the balance of the bike - the balance angle. BikeController uses the balance angle to control the bike. Obviously, the balance angle depends on the mechanical properties of the bike.

Let<br>
> s - steering angle.<br>
t -  target steering angle.<br>
b - balance steering angle.<br>
d - dumper. This value depends on the angle of inclination and the rate of change of this angle. It should be noted that dumper is not a magical power. Dumper is applied not to the bike, but to the steering angle.<br>
k - is a constant coefficient.<br>

Then the steering angle is calculated using the following formula:<br>

s = b + (b - t) * k + d

As a result, the bike tilts towards the turn until the factor (b - t) becomes zero.

**Properties**
- **frontCollider** - 
Front [WheelCollider](https://docs.unity3d.com/Manual/class-WheelCollider.html)
- **rearCollider** - 
Rear [WheelCollider](https://docs.unity3d.com/Manual/class-WheelCollider.html)
- **maxSteer** -
Limits steering angle
- **maxLean** -
Limits bike lean
- **centerOfMassY** -
Determines the height of the center of mass above the ground. Lowering the center of mass makes the bike more stable.
- **curves** -
Curves are generated automatically at runtime.
- **info** -
These fields are calculated automatically at runtime.

**Public Methods**
- **getBalanceSteer** -
Returns the steer angle required to maintain balance at the current speed and the current lean.
- **setAcceleration** -
Sets rear wheel torque acording to the given acceleration.
- **frontBrake** -
Sets front brake according to the given acceleration. Clamps acceleration to minimize slipping.
- **rearBrake** -
Sets rear brake according to the given acceleration. Clamps acceleration to minimize slipping.
- **safeBrake** -
Sets both brakes according to the given acceleration. Clamps the brakes to minimize slip and prevent rollovers.
- **releaseBrakes** -
Releases both brakes.
- **setSteerDirectly** -
Sets steering angle to given.
- **setLean** -
Brings the bike closer to the desired lean by slightly off balance. This method is useful for high speed control.
- **setSteer** -
Brings the steer angle closer to the required steer by a small deviation from the balance steer.
- **setSteerByLean** -
Brings the steer angle closer to the required steer by a small deviation from the balance steer. First calculate lean then call setLean.
- **getSidewaysFriction** -
BikeController changes [SidewaysFriction](https://docs.unity3d.com/ScriptReference/WheelCollider-sidewaysFriction.html) depend on velocity. This method returns [SidewaysFriction](https://docs.unity3d.com/ScriptReference/WheelCollider-sidewaysFriction.html) for the given speed.
- **getLean** -
Returns current lean.
- **getMaxForwardAcceleration** -
Returns max forward acceleration. Acceleration is limited by slipping and the possibility of rolling over.
- **getMaxBrakeAcceleration** -
Returns max brake acceleration. Acceleration is limited by slipping and the possibility of rolling over.
- **getMaxSidewaysAcceleration** -
Returns max sideways acceleration for the given velocity. Acceleration is limited by sideways friction.
- **getMaxVelocity** -
Returns the maximum velocity for a given turning radius.
- **reset** -
Returns bike to the starting position.
- **getup** -
Getting bike up. Use if the bike falls over.
- **getHitPoint** -
Returns the midpoint between the front and back touch points.
<a name="feet"></a>
#### 1.1.2 Feet
**Description**<br>
Feet is a pair of feet, each of which consists of three parts: Rigidbody, SphereCollider and ConfigurableJoint connected to the bike. The foot serves as a support when the bike's tilt becomes dangerous.
The second function of the leg is to capture and hold the ball when playing motorball.

**Properties**
- **ball**: The ball transform for motoball.
- **footTargetPosition**: Target position for the feet in wait mode.
- **footDown**: Enables support behavior.
- **waitStart**: Sets the wait behavior.
- **motoball**: Activates the motorball behavior.

**Public Methods**
- **reset**: Sets waitStart to true, indicating that the system is waiting to start.
- **start**: Sets waitStart to false, indicating that the system has started.
- **getStateL**: Returns the state of the left foot.
- **getStateR**: Returns the state of the right foot.
- **getLeftFoot**: Returns the left foot object.
- **getRightFoot**: Returns the right foot object.


<a name="input"></a>
### 1.2 Input

<a name="manualcontrol"></a>
#### 1.2.1 ManualControl
**Description**<br>
ManualControl receives data from the [BikeInput](bikeInput) script and controls the [BikeController](bikecontroller) script using apropriate methods.

Input data:
- X axis
- Y axis

Output data:
- Target steering angle
- Forward or brake  acceleration

**Properties**
- **sliderX** -
Optional field. The slider visualizes user input along the X axis.
- **sliderSteer** -
Optional field. The slider visualizes the current steering angle.
- **bikeController** -
[BikeController](#bikecontroller)
- **timeScale** -
Sets the [Time.timeScale](https://docs.unity3d.com/ScriptReference/Time-timeScale.html);
- **maxVelocity** -
Scale of X axis. If X axis = 1 velocity = maxVelocity.
- **fullAuto** -
If true, balance is carried out automatically else the user must balance manually. In the last case, steering angle calculated as mix between user input and balanced steering angle + dumper.
- **autoBalance** -
The interpolation value between user input and balanced steering angle.
- **damper** -
Damper factor.
- **info** -
These fields are calculated automatically at runtime.
<a name="bikeinput"></a>
#### 1.2.2 BikeInput
**Description**<br>
BikeInput supports old and new Input System. If new Input System available user can select one of InputDevece from [InputSystem.devices](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.InputSystem.html#properties). BikeInput supports the following devices: [Keyboard](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.Keyboard.html), [Mouse](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.Mouse.html), [Joystick](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.Joystick.html) and [Gamepad](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.Gamepad.html).
If new Input System not available BikeInput use [Input.GetAxis](https://docs.unity3d.com/ScriptReference/Input.GetAxis.html) method.
In either case, user input is placed in the xAxis and yAxis fields.

**Properties**
- **keyboardSettings** -
For new input system only.
- **joystickSettings** -
For new input system only.
- **sensitivity** -
User input sensitivity.
- **toZero** -
Determines the speed at which the return to zero occurs.
- **xAxis** -
Output of this script.
- **yAxis** -
Output of this script.

<a name="devicedropdown"></a>
#### 1.2.3 DeviceDropdown
**Description**<br>
For new input system only. This script allow you to select one of the available Input Devices.

**Properties**
- **deviceDropdown** -
This [Dropdown](https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/script-Dropdown.html) will be contain available Input Devices.
- **bikeInput** -
[BikeInput](#bikeinput) script.

<a name="parts"></a>
### 1.3 Bike parts

This section contains a description of scripts that carry out the movement of some parts of the bike. Front and rear forks, wheels, etc. Each script must be attached to the corresponding bike model object.
Each script moves several parts of the bike. So FrontFork turns the fork left and right, moves the wheel axle along the fork and rotates the wheel.

There are two ways to connect a visual model to a physical one.
1. Attach visual objects to physical ones.
2. Fill in the script field for visual objects.
If you have attached visual objects, then there is no need to fill out the fields. If you filled in the fields of visual objects, the script will attach them during execution.
The second method is convenient if you edit a visual model in Blender, for example. When you export fbx to an open Unity project, changes immediately appear on the scene. You don't need to re-drag the objects.

The sequence of building a bike model.
1. Place one of the bike prefabs on the scene.
2. Unpack the prefab (not completely).
3. Remove visual objects.
4. Drag the visual model (.fbx file) into your model.
5. Fill in the fields of visual objects in scripts or unpack fbx and drag objects into your model.

<a name="fork"></a>
#### 1.3.1 Fork
**Description**<br>
Fork is designed to visualize a telescopic fork. The [WheelCollider](https://docs.unity3d.com/Manual/class-WheelCollider.html) damper moves vertically, but telescopic fork has some incline. Fork performs appropriate motion of the damper and wheel.

**Properties**
- **frontCollider** -
front [WheelCollider](https://docs.unity3d.com/Manual/class-WheelCollider.html)
- **fork** -
The upper part of the fork.
- **axis** -
The lower part of the fork. The moving part of the dumpers and the wheel axis are located here.
- **frontWheel** -
Front wheel object.
- **forkModel** -
Front fork visual model object.
- **frontAxisModel** -
Front axis visual model object.
- **frontWheelModel** -
Front wheel visual model object.

<a name="swingarm"></a>
#### 1.3.2 Swingarm
**Description**<br>
Swingarm provides the movement of the swingarm and rear wheel.

**Properties**
- **rearCollider** -
Rear [WheelCollider](https://docs.unity3d.com/Manual/class-WheelCollider.html).
- **wheel** -
Rear wheel object.
- **rearWheelModel** -
Rear wheel visual model object.
- **swingarmModel** -
Rear fork visual model object.

<a name="wheel"></a>
#### 1.3.3 Wheel
**Description**<br>
Wheel provides the movement of the wheel. If Children contains a [TrailRenderer](https://docs.unity3d.com/Manual/class-TrailRenderer.html), the script controls it.

**Properties**
- **wheelCollider** -
- **wheelVisualModel** -

<a name="damper"></a>
#### 1.3.4 Damper
**Description**<br>
Damper provides the movement of the rear dumper.

**Properties**
- **damperBottom** -
Bottom of the damper. This part is located on the swingarm.
- **spring** -
Damper spring
- **springLength** -
Length of the spring
- **modelTop** -
Damper top visual model
- **modelBottom** -
Damper bottom visual model
- **modelSpring** -
Spring visual model

<a name="chain"></a>
#### 1.3.5 Chain
**Description**<br>
The Chain script moves the chain. Different sections of the chain move in different ways. In this regard, the chain Mesh is divided into four parts, each of which moves in a corresponding way. Movement is limited by the length of one chain link.

In order to prepare the script for work, you need to complete the following steps:
1. Move the Chain to the front sprocket position.
2. Fill in the fields of the script. The frontSprocket and frontSprocketModel fields are optional.
3. Click the Look at Wheel button.
4. Fill in the Chain field. There should be a visual object here.
5. Click the Detect Radius button. As a result, the R1, R2 fields will be filled in and the outline of the chain, drawn in blue, will appear in the scene window. In addition, red dots will appear at the top of the chain to indicate chain links.
6. Select Chain Pitch from the drop down menu. The pitch of the red dots must match the pitch of the chain. You can edit the Chain Pitch  field;
7. Click the Subdivide Mesh button. As a result, four new meshes will be generated, four new objects will be added to the Chain object, and the old mesh will be deactivated.
8. Move the slider. You will see the chain moving. Install the chain so that it fits correctly on the sprocket.
9. You can save the new mesh. To do this, specify the Mesh Path and click the Save Mesh button.

**Properties**
- **rearCollider** -
Rear [WheelCollider](https://docs.unity3d.com/Manual/class-WheelCollider.html).
- **rearWheel** -
Rear wheel [Transform](https://docs.unity3d.com/ScriptReference/Transform.html).
- **chain** -
Chain visual object
- **frontSprocketModel** -
Front sprocket visual object
- **R1** -
Front sprocket radius
- **R2** -
Rear sprocket radius
- **Chain pitch** -
Chain pitch
- **Double pitch** -
The chain pitch is equal to the length of two links.
- **Offset** -
Longitudinal chain offset
- **Mesh path** -
Path to save mesh

**Public Methods**
- **rotateChain** -
Moves the chain in accordance with the specified angle of rotation of the rear sprocket. This method must be called by a script that rotates the rear wheel. [Swingarm](#swingarm) script, in our case.

<a name="pedals"></a>
#### 1.3.6 Pedals
**Description**<br>
Rotates the drive sprocket of the bicycle in accordance with the rotation of the rear wheel. The gear ratio changes automatically to keep the rotation speed within the specified limits.
Must be attached to [WheelCollider](https://docs.unity3d.com/Manual/class-WheelCollider.html). [TrailRenderer](https://docs.unity3d.com/Manual/class-TrailRenderer.html) and [ParticleSystem](https://docs.unity3d.com/ScriptReference/ParticleSystem.html) are located in children.

**Properties**
- **rearCollider** -
Rear [WheelCollider](https://docs.unity3d.com/Manual/class-WheelCollider.html).
- **pedals** -
Drive sprocket visual object.
- **minRps** -
Minimum rotation speed in revolutions per second.
- **maxRps** -
Maximum rotation speed in revolutions per second.

<a name="other"></a>
### 1.4 Other Bike scripts

<a name="biketrackcontroller"></a>
#### 1.4.1 BikeTrackController
#### Description
The BikeTrackController class is an implementation of the TrackController class. The Bike script should be attached as the IVehicle.
BikeTrackController guides the bike along the track. The bike is represented by the Bike class and the track by the TreckSpline class.

The bike tends to swing around the target object. A damper is used to prevent swinging. The damper is applied to the target steering angle.

#### Fields:
- **zSpeedValue** - Value for incline dumper interpolation between rotation speed and rotation value.
- **zDumper** - Damping factor for z-axis rotation.
- **trackSteer** - Rudder interpolation between interpolated track direction and direction to target.
- **drift** - Limits acceleration when drifting.

<a name="slipeffects"></a>
#### 1.4.2 SlipEffects
**Description**<br>
Emits trail and particles depending on wheel slip and specified parameters.

**Properties**
- **useParticles** -
Splashes or smoke from under the rear wheel
- **minParticleSlip** -
Wheel slip threshold for particles.
- **backward** -
Emit trail When slipping backward.
- **forward** -
Emit trail When bracking.
- **sideways** -
Emit trail When slipping sideways.
- **minTrailSlip** -
Wheel slip threshold for trail.

<a name="sound"></a>
#### 1.4.3 Sound
**Description**<br>
If rps < 0.001 plays idle [AudioClip](https://docs.unity3d.com/2019.4/Documentation/Manual/class-AudioClip.html), else plays run [AudioClip](https://docs.unity3d.com/2019.4/Documentation/Manual/class-AudioClip.html).
Pitch and volume depends on [WheelCollider](https://docs.unity3d.com/ScriptReference/WheelCollider.html) [rpm](https://docs.unity3d.com/ScriptReference/WheelCollider-rpm.html) and [motorTorque](https://docs.unity3d.com/ScriptReference/WheelCollider-motorTorque.html).

**Properties**
- **idle** -
Idle [AudioSource](https://docs.unity3d.com/ScriptReference/AudioSource.html)
- **run** -
run [AudioSource](https://docs.unity3d.com/ScriptReference/AudioSource.html)
- **bikeController** -
[BikeController](#bikecontroller)
- **runRPS** -
RPS of run AudioClip
- **gearRatio** -
Full gear ratio
- **pitch** -
Info field
#### Control keys
- **O key** - Sound on/off.

<a name="wheelColliderInterpolator"></a>
#### 1.4.4 WheelColliderInterpolator
**Description**<br>
[WheelCollider](https://docs.unity3d.com/ScriptReference/WheelCollider.html) does not provide interpolation capabilities. This script interpolates WheelCollider data.

**Properties**
- **posInterpolated** -
Interpolated wheel position
- **rotInterpolated** -
Interpolated wheel rotation
- **steerInterpolated** -
Interpolated steering angle

**Public Methods**
- **getWorldPose** -
Returns the interpolated position and rotation of the wheel.
- **getSteer** -
Returns the interpolated steer angle.
- **reset** -
Sets all variables to current value.

<a name="biker"></a>
## 2 Biker

<a name="ikcontrol"></a>
#### 2.1 IKControl
**Description**<br>
This script provides full-body IK (Inverse Kinematics) control for a character riding a bike, bicycle, or in a motoball game. It allows the character to interact realistically with the bike, including leaning, pedaling, and foot positioning.

**Properties**
- **lookAt (bool)** - Enable/disable head tracking to look at a target.
- **target (Transform)** - The target for the character to look at (if lookAt is enabled).
- **ball (Transform)** - The ball object for motoball mode.
- **accelerator (bool)** - Enable/disable accelerator control.
- **Break (bool)** - Enable/disable brake control.
- **leftHandHandle (Transform)** - The left hand handle for IK.
- **rightHandHandle (Transform)** - The right hand handle for IK.
- **leftFootHandle (Transform)** - The left foot handle for IK.
- **rightFootHandle (Transform)** - The right foot handle for IK.
- **getOnFootpegs (bool)** - Enable/disable getting on footpegs.
- **bikerRigidbody (Rigidbody)** - The Rigidbody of the character.
- **bikeRigidbody (Rigidbody)** - The Rigidbody of the bike.
- **mode (Enum)** - The mode of the character (Bike, Bicycle, Motoball).
- **frame (Transform)** - The frame of the bike (for visual purposes).
- **leftPedalFootHandle (Transform)** - The left pedal foot handle.
- **rightPedalFootHandle (Transform)** - The right pedal foot handle.
- **leftPedalVisualModel (Transform)** - The visual model for the left pedal.
- **rightPedalVisualModel (Transform)** - The visual model for the right pedal.
- **shiftUpAcc (float)** - The acceleration threshold for shifting up.
- **shiftHeight (float)** - The height for shifting up.
- **onLeanAction (Enum)** - Action to take on leaning (None, LeftRightMotion, FootDown, Both).
- **spineToSteer (float)** - The amount of spine rotation for steering.
- **hipsRotationX (float)** - The initial rotation of the hips on the X axis.
- **spineRotationX (float)** - The initial rotation of the spine on the X axis.
- **lElbow (Transform)** - The left elbow transform for IK hints.
- **rElbow (Transform)** - The right elbow transform for IK hints.
- **lKnee (Transform)** - The left knee transform for IK hints.
- **rKnee (Transform)** - The right knee transform for IK hints.
- **inputData (InputData)** - Input data for the IK control.
<a name="footContact"></a>
#### 2.2 FootContact
**Description**<br>
This class tracks the contact of a foot with a surface.
**Properties**
- **collisionStay** - Indicates if the collision with the surface persists.
- **contactPoint** - [ContactPoint](https://docs.unity3d.com/ScriptReference/ContactPoint.html)

<a name="track"></a>
# 3 Track

<a name="spline"></a>
## 3.1 Spline

<a name="splinesegment"></a>
### SplineSegment Class
Represents a segment of a spline curve in 3D space.

#### Constructors

- **SplineSegment(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)** - Constructs a spline segment using control points.
- **SplineSegment(Vector3 p0, Vector3 p3)** - Constructs a spline segment using only the starting and ending control points. The intermediate control points are calculated automatically.
- **SplineSegment()** - Constructs a default spline segment with all control points at the origin.

#### Public Methods

- **void addY(float value)** - Adds a value to the y-coordinate of all control points.
- **void setP0y(float value)** - Sets the y-coordinate of the starting control point.
- **void setP1y(float value)** - Sets the y-coordinate of the first control point.
- **void setP2y(float value)** - Sets the y-coordinate of the second control point.
- **void setP3y(float value)** - Sets the y-coordinate of the ending control point.
- **float getS(float t)** - Gets the arc length corresponding to the parameter t.
- **float getT(float s)** - Gets the parameter value corresponding to the given arc length.
- **Vector3 GetPoint(float t)** - Gets a point on the spline curve corresponding to the parameter t.
- **Vector3 GetPointL(float l)** Gets a point on the spline curve corresponding to the arc length l.
- **Vector3 getDerivate1(float t)** - Gets the first derivative (tangent) of the spline curve at parameter t.
- **Vector3 getDerivate2(float t)** - Gets the second derivative of the spline curve at parameter t.
- **float getLength(float t)** - Gets the arc length of the spline curve up to parameter t.
- **void getLeftRight(float t, out Vector3 left, out Vector3 right)** - Gets the left and right directions perpendicular to the tangent of the spline curve at parameter t.
- **Vector3 getCurvatureVector2d(float t)** - Gets the curvature vector in 2D space at parameter t.
- **Vector3 getCurvatureVector3d(float t)** - Gets the curvature vector in 3D space at parameter t.
- **float getCurvatureRadius2d(float t)** - Gets the curvature radius in 2D space at parameter t.
- **float getCurvatureRadius3d(float t)** - Gets the curvature radius in 3D space at parameter t.
- **float getSignedRadius2d(float t)** - Gets the signed curvature radius in 2D space at parameter t.
- **Vector3 getNormalAcceleration(float t, float velocity)** - Gets the normal acceleration of a point on the spline curve at parameter t and given velocity.
- **void getTrackSlope(float t, float velocity, out Vector3 left, out Vector3 right)** - Gets the slope of the track surface at parameter t for a given velocity.
- **Vector3 getClosest(Vector3 point, out float t)** - Gets the point on the spline curve closest to the given point in 3D space.
- **void update(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)** - Updates the control points of the spline segment.
- **void updateLenght()** -Recalculates the length and other properties of the spline segment.

#### Public Properties

- **Vector3 P0** - Gets or sets the starting control point.
- **Vector3 P1** - Getsor sets the first control point.
- **Vector3 P2** - Gets or sets the second control point.
- **Vector3 P3** - Gets or sets the ending control point.
- **float length** - Gets the length of the spline segment.
- **float offsetInSpline** - Gets or sets the offset of the spline segment within the overall spline.
- **float minRadius** - Gets the minimum radius of curvature within the spline segment.

<a name="splinebase"></a>
### SplineBase Class
A class representing the base spline curve.

#### Fields

- **segments: List<SplineSegment>** - list of spline segments.
- **clockwise: bool** - indicates the direction of the spline.
- **count: int** - number of segments.
- **radius: float** - spline radius.
- **length: float** - spline length.
- **minLength: float** - minimum spline length.
- **l2segN: int[]** - array for calculating spline point by length.
- **turns: List<Turn>** - list of turns on the spline.
- **minTurnLength: float** - minimum turn length.

#### Methods

- `init(): void` - initializes the spline.
- `getSegT(float s, out int seg): float` - gets the parameter t for the given length s.
- `getPoint(float s): Vector3` - returns the point on the spline for the given length s.
- `getDerivate1(float s): Vector3` - returns the first derivative at point s.
- `getCurvatureRadius2d(float s): float` - returns the curvature radius of the spline in 2D for the given length s.
- `getSignedRadius2d(float s): float` - returns the signed curvature radius of the spline in 2D for the given length s.
- `getCurvatureRadius3d(float l): float` - returns the curvature radius of the spline in 3D for the given length l.
- `getCurvatureVector2d(float s): Vector3` - returns the curvature vector of the spline in 2D for the given length s.
- `getLeftRight(float s, out Vector3 left, out Vector3 right): void` - returns the left and right points of the spline for the given length s.
- `getTurn(float s): Turn` - returns the turn for the given length s.
- `getFromToS(float fromS, float toS): float` - returns the distance between two points on the spline.
- `getClosestDistance(float fromS, float toS): float` - returns the closest distance between two points on the spline.
- `getClosestL(Vector3 point, out float closestL, float lastL = -1, float clamp = 4): Vector3` - returns the closest point on the spline to the given point.
- `getLength(): float` - returns the length of the spline.
- `updateLength(bool soft = false): void` - updates the length of the spline.
- `updateTurns(float minRadius, float minTurnAngle, float trackWidth, bool mergeTurns): void` - updates the turns on the spline.
- `nextI(int i, int count = -1): int` - returns the next index.
- `prevI(int i, int count = -1): int` - returns the previous index.
- `clampS(float s, float length): float` - clamps the spline length value.

### Internal Classes

### SPoint

- Represents a point on the spline.
- `seg: int` - segment index.
- `t: float` - parameter t.
- `s: float` - spline length.
- `ss: SplineSegment` - spline segment.
- `scalarRadius: float` - scalar radius.
- `radius: Vector3` - radius.
- `setCenterXZ(SPoint prevSP, SPoint nextSP): void` - sets the spline center in the XZ plane.
- `getPos(): Vector3` - returns the point position.
- `getTangent(): Vector3` - returns the tangent vector.

### SP1

- Represents a point on the spline.
- `seg: int` - segment index.
- `t: float` - parameter t.
- `s: float` - spline length.
- `l: float` - length.

### Turn

- Represents a turn on the spline.
- `s: float` - spline length.
- `startS: float` - start length of the turn spline.
- `endS: float` - end length of the turn spline.
- `leftRight: int` - turn direction (1 - left, -1 - right).
- `position: Vector3` - turn position.
- `startPoint: Vector3` - start point of the turn.
- `endPoint: Vector3` - end point of the turn.
- `radius: float` - turn radius.
- `signedRadius: float` - signed turn radius.
- `smoothRadius: float` - smoothed turn radius.
- `maxV: float` - maximum velocity.
- `angle: float` - turn angle.
- `updateVectors(SplineBase spline): void` - updates vectors for the turn.

<a name="spline1"></a>
### Spline1 Class
A class representing a specific type of spline curve, inheriting from `SplineBase`.

#### Constructor

- `Spline1(int count, float radius, bool clockwise = false)`: Initializes a new instance of the `Spline1` class with the specified number of segments, radius, and direction.

#### Methods

- `reset(int count, float radius, bool clockwise = false)`: Resets the spline with the specified number of segments, radius, and direction.
- `setP0(Vector3 pos, int index)`: Sets the position of control point P0 for the segment at the specified index.
- `setP1(Vector3 pos, int index)`: Sets the position of control point P1 for the segment at the specified index.
- `setP2(Vector3 pos, int index)`: Sets the position of control point P2 for the segment at the specified index.
- `setTangent(int index)`: Sets the tangent for the segment at the specified index.
- `addSegment(int index)`: Adds a new segment after the segment at the specified index.
- `removeSegment(int index)`: Removes the segment at the specified index.

<a name="spline2"></a>
### Spline2 Class
A class representing another type of spline curve, inheriting from `SplineBase`.

#### Properties

- `List<Boor> boor`: A list of control points (Boor objects) for the spline.

#### Constructor
- `Spline2(int count, float radius, float randomH, float randomV)`: Initializes a new instance of the `Spline2` class with the specified number of segments, radius, and randomness parameters.

#### Methods

- `reset(int count, float radius, float randomH, float randomV)`: Resets the spline with the specified number of segments, radius, and randomness parameters.
- `update(bool soft = false)`: Updates the spline, optionally with soft updating.
- `createFromTurns(SplineBase spline, float width)`: Creates a spline from the turns of another spline with a specified width.
- `fitToPeaks(SplineBase spline, SplineBase splineL, SplineBase splineR, float width, float peacksInOut)`: Fits the spline to peaks based on other splines with specified width and peak parameters.
- `setPos(Vector3 pos, int index)`: Sets the position of the control point at the specified index.

### Boor Class

- `Boor(Vector3 p)`: Initializes a new instance of the `Boor` class with the specified position.

<a name="track-spline"></a>
## 3.2 Track Spline
This section describes scripts that allow you to build tracks. There are two scripts that allow you to build track splines. TreckSpline and TreckSpline2.
TreckSpline generates a spline whose segments are connected in a smooth manner. At the junction point, the first derivatives coincide. In the case of TreckSpline2, the second derivatives are also the same. TreckSpline allows you to move a node directly, TreckSpline2 does not.
A bike, unlike a car, cannot change its steering angle instantly, so it cannot travel exactly along the TreckSpline path. Along the TreckSpline2 trajectory - can.
For your track, you can use TreckSpline or both splines. With TreckSpline you can build an exact replica of the original race track, then use TreckSpline2 to plot the optimal path along the track. See Motocross example.

<a name="tracksplinescript"></a>
### 3.2.1 TrackSpline Script

#### Description
TreckSpline allows you to create and edit a track. The track consists of several splines [Spline1](#spline1).
#### Properties
- **settings**: `Settings` - Settings object containing parameters for configuring the track spline. These parameters are used when you press the Reset button

![Settings](https://github.com/V-Kudryashov/md-Files/assets/17885189/0429826a-a172-4d6a-b768-3a8cedba526c)

- **jumps**: `List<Jump>` - List of Jump objects representing jumps on the track spline. You can manually add elements to this array and move them around the track.

![Jumps](https://github.com/V-Kudryashov/md-Files/assets/17885189/e54664ec-a464-47a3-8b33-c62c4e9ed5af)

- **showTrack** - Enabling this property allows you to see and edit spline elements.
    - **curves** - Three splines: Central, left and right.
    - **nodes** - Spline nodes. You can edit them.
    - **turns** - Generated automatically. Used by Trackcontroller.
    - **jumps** - You can add them and edit them. Used by Trackcontroller.
    - **start** - Start zone. Initially located at the beginning of the track. The length is zero. Used when building Mesh;
    - **start tracks** - The zone next to the starting one. In this zone, riders stick to their own tracks. Used by TrackСontroller.

![ShowTrack](https://github.com/V-Kudryashov/md-Files/assets/17885189/0ff84c93-85f3-4890-ad66-2c9dc4829af6)

- **selectedNode** - The spline node you selected in the scene.

![SelectedNode](https://github.com/V-Kudryashov/md-Files/assets/17885189/e822c10b-6132-4306-8c62-23046bd347d1)

- **moreOptions** -
    - **Update Spline batton**: Updates some spline variables that are not updated automatically. These are lengths, turns, etc.
    - **Reset tangents button**: Sets tangents in the optimal direction.
    - **Reset width button**: Sets the width of all nodes according to the Settings field.
    - **Reset hills button**: Resets the width and tangents of the hills.

![MoreOptions](https://github.com/V-Kudryashov/md-Files/assets/17885189/7ad76c7d-22e6-4775-b959-bade28740051)

<a name="trackspline2"></a>
### 3.2.2 TrackSpline2

#### Description
The TreckSpline2 script is intended for use in conjunction with TreckSpline. Allows you to create a smooth track based on a TreckSpline track.

![TrackSpline2](https://github.com/V-Kudryashov/md-Files/assets/17885189/27701f65-a9f1-4ee8-a740-b5eabd2719a0)

- **'Reset' button** - creates a new spline in accordance with the Settings parameters.
- **'Fit turns to peaks' button** - moves the spline closer to the inner or outer edge of the turn.
- **'Peaks in-out' slider** - defines the position between the inner and outer edges of the turn.

<a name="trackterrain"></a>
## 3.3 TrackTerrain

#### Description
This class is responsible for managing Terrain texture and Terrain heights.

![TrackTerrain](https://github.com/V-Kudryashov/md-Files/assets/17885189/e9736a1b-4dcf-4712-93b6-0ed58d2afd1a)

#### Fields:
    
- **spline (TrackSpline)** - A reference to the TrackSpline object.
- **terrain (Terrain)** - A reference to the Terrain object.
- **clearHeights (bool)** - A flag indicating whether to clear the terrain heights before building the track.
- **landTrack (bool)** - A flag indicating whether to "land" the track on the terrain.
- **borderWidth (float)** - The width of the border around the track.

#### Buttons:

- **Build terrain** - Builds a track on the terrain according to the spline.
- **Draw track** - Draws a track on the terrain.
 
<a name="trackmesh"></a>
## 3.4 TracMesh

#### Description
The TrackMesh class generates a Mesh representing a TrackSpline.

![TrackMesh](https://github.com/V-Kudryashov/md-Files/assets/17885189/610381be-e61e-419b-8211-5800f9b5259c)


#### Fields:
- **terrain** - The Terrain on which the track is placed.
- **resolution** - The resolution of the track mesh.
- **yOffset** - Vertical offset of the track from the terrain.
- **trackWidth** - Width of the track Mesh. The remaining part is occupied by the roadside.
- **height** - Height of the track.
- **markerWidth** - Width of the track markers.
- **landRoadside** - Whether to land the roadside of the track.
- **landUnderside** - Whether to land the underside of the track.

#### Materials

![Mtaterials](https://github.com/V-Kudryashov/md-Files/assets/17885189/ebcac888-e7e9-4794-b424-2bd15e0e096e)

The Mesh contains 5 sub-meshes, for which you need to provide 5 materials. See Suzuka example.
1. Road
2. Roadside
3. Marker.
4. Track body.
5. Starting area.

<a name="trackcontroller"></a>
## 3.5 TrackController

#### Description
The TrackController script drives a vehicle along the track. This vehicle must implement the IVehicle interface. It could be a Bike, a Segway, or something else. TrackController determines the direction of movement and speed.

A target object moves along the track 1.5 seconds ahead of the bike, and the bike follows it. In addition, the track direction and radius of curvature are used to maintain the correct direction.

#### Fields:
- **trackSpline** - Reference to the TrackSpline component defining the track path.
- **closest1GO** - GameObject representing the closest point on the track spline.
- **closest2GO** - GameObject representing the closest point on the second track spline.
- **target** - Target GameObject for the bike to steer towards.
- **speedText** - Text component for displaying speed and other information (optional).
- **score** - Instance of the Score class for tracking lap times and speed statistics.
- **maxVelocity** - Maximum velocity the bike can reach.
- **speed** - Speed factor for controlling bike velocity.
- **tangentRadius** - Rudder interpolation between track tangent and turning radius.
- **targetTimeout** - Time limit for reaching the target.
- **useSmoothRadius** - Flag for using smooth radius for calculations.
- **useConstantVelocity** - Flag for using a constant velocity instead of calculated velocity.
- **constantVelocity** - Constant velocity value when useConstantVelocity is true.
- **manualSteer** - Manual steering input (0-1).
- **manualVelocity** - Manual velocity input (0-1).
- **blending** - Blend mode for blending manual and calculated values.

### Public Methods

- **init(IVehicle vehicle)**: Initializes the track controller with a vehicle implementing the `IVehicle` interface.
- **update()**: Updates the track controller, including updating the speed text display.
- **fixedUpdate()**: Updates the track controller in a fixed time step, including updating the closest points on the track and the target position.
- **start()**: Starts the track controller, resetting the score and setting the wait flag to false.
- **reset()**: Resets the track controller, including resetting the vehicle and score.
- **getJump()**: Returns the current jump on the track.
- **getTurn()**: Returns the current turn on the track.
- **getRadiusSteer(float s)**: Returns the steering angle based on the track's radius at a given point.
- **getTangentSteer(float s)**: Returns the steering angle based on the track's tangent at a given point.
- **getTracktSteer(float s)**: Returns the steering angle based on a blend of the tangent and radius at a given point.
- **getSteerToTarget()**: Returns the steering angle to the target position.
- **getSafeVelocity()**: Returns the safe velocity for the vehicle.
- **getL()**: Returns the current position on track.
- **getL2()**: Returns the current position track2 length.

### Serialized Classes

### Score
- **lap** *(int)*: The current lap number.
- **lapTime** *(float)*: The time taken for the current lap.
- **time** *(float)*: The current time.
- **speed** *(float)*: The current speed of the vehicle.
- **lapMaxSpeed** *(float)*: The maximum speed achieved in the current lap.
- **maxSpeed** *(float)*: The maximum speed achieved overall.
- **maxSpeedS** *(float)*: The track зщышешщт at which the maximum speed was achieved.

### DispatcherData
- **wait** *(bool)*: Flag indicating if the vehicle should wait.
- **currentSpeed** *(float)*: The current speed factor.
- **startingL** *(float)*: The starting position.
- **l** *(float)*: The track length from the beginning of the spline to the current position.
- **fullL** *(float)*: Distance traveled.
- **s** *(float)*: The length of the track from the start to the current position.
- **fullS** *(float)*: Distance traveled.
- **zeroCounter** *(int)*: Counter for zero crossings.
- **lapCounter** *(int)*: Counter for lap crossings.
- **distanceToRedLight** *(float)*: Distance to the red light.
- **stopline** *(float)*: The position of the stop line.


<a name="trackdispatcher"></a>
## 3.6 TrackDispatcher

#### Description
The `TrackDispatcher` class manages the behavior of vehicles on a track. It controls various aspects such as speed, collision avoidance, and interaction with traffic lights.

#### Fields
- **spline**: A reference to the TrackSpline component representing the track.
- **timeScale**: Controls the overall time scale of the simulation.
- **slowJump**: Determines if the simulation should slow down when the bike jumps.
- **jumpTimeScale**: Time scale applied during jumps.
- **toCenter**: Adjusts the speed of the bike so that the bike approaches the midpoint.
- **randomSpeed**: Random speed variation applied to the bike.
- **avoidCollisions**: Determines the strength of collision avoidance behavior.

#### Control keys
- **S key** - Starts all bikes on the track.
- **R key** - Resets all bikes on the track.
- **P key** - Switches the control to the next bike.
- **T key** - Switches the timeScale to slow or normal.

<a name="IVehicle"></a>
## 3.7 IVehicle
### Description
The IVehicle interface allows the TrackCintroller class to control various vehicles such as a bicycle, segway, etc.
### Public Methods
- **public Rigidbody getRigidbody()** - Gets the Rigidbody component of the vehicle.
- **public float getMaxForwardAcceleration()** - Gets the maximum forward acceleration of the vehicle.
- **public float getMaxBrakeAcceleration()** - Gets the maximum brake acceleration of the vehicle.
- **public float getMaxSidewaysAcceleration(float velocity)** - Gets the maximum sideways acceleration of the vehicle at a given velocity.
- **public float getMaxVelocity(float radius)** - Gets the allowed speed for the given turning radius.
- **public float getRadiusSteer(float radius)** - Gets the radius steer value for the given turning radius.
- **public float getTurnRadius()** - Gets the current turning radius of the vehicle.
- **public float getTurnDir()** - Gets the current turning direction of the vehicle.
- **public void reset()** - Resets the vehicle to its initial state.
- **public void getup(float h = 0.1f, float turn = 0)** - Sets the vehicle to a vertical position. **h** - The height to which the vehicle should be lifted. **turn** - The turn value to set.

<a name="actions"></a>
### 3.8 Actions

### Description
The `Actions` script provides functionality for controlling various aspects of the game, such as switching cameras, adjusting time scale, and toggling sound.

### Public Variables
- **panelMenu** *(GameObject)*: The menu panel GameObject.
### Public Methods
- **showHidePanelMenu()**: Toggles the visibility of the menu panel.
- **selectNext()**: Selects the next camera in the list.
- **select(int index)**: Selects the camera at the specified index.
- **switchCamera()**: Switches the current camera.
- **zoomPlus()**: Zooms in the current camera.
- **zoomMinus()**: Zooms out the current camera.
- **sound()**: Toggles sound on/off.
- **updateTimeScale()**: Toggles slow motion.

## Usage
Attach this script to a GameObject in your scene. Assign the `panelMenu` variable to the menu panel GameObject. Assign buttons actions to the script public methods.
<a name="trafficlights"></a>
## 3.9 TrafficLights

<a name="trafficlight"></a>
### 3.9.1 TrafficLight
#### Description
The TrafficLight script controls the behavior of the attached TrafficLightBoxes. It manages the phases of the traffic light (red, yellow1, yellow2, green) and assigns the corresponding colors to associated tracks.
#### Enumerations
- **TrafficColor**: Represents the possible colors of a traffic light (Red, Yellow, Green).
- **Phases**: Represents the phases of the traffic light (Yellow1, Red, Yellow2, Green).
#### Fields
- **dir1**: A Direction object representing settings for the first direction controlled by the traffic light.
- **dir2**: A Direction object representing settings for the second direction controlled by the traffic light.
- **tracks**: A list of Track objects representing the tracks associated with the traffic light.
- **color1**: The current color of the traffic light for direction 1.
- **color2**: The current color of the traffic light for direction 2.

![TrafficLight](https://github.com/V-Kudryashov/md-Files/assets/17885189/8f28e35a-3ff0-4a85-ab66-be6781cd9b7e)

#### Classes
- **Direction**: A class representing settings for a traffic light direction, including green time, brake time, and stop line position.
- **Track**: A class representing a track associated with the traffic light, including the track object, direction, position on the track, stop line position, and current traffic light color.
#### Usage
- Attach the TrafficLight script to a GameObject representing the traffic light.
- Set the dir1 and dir2 variables in the inspector to configure the green time, brake time, and stop line for each direction.
- Add Track objects to the tracks list in the inspector to associate them with the traffic light.
- Add child objects and attach TrafficLightBox scripts to them.
You can see an example of use in the City scene. Assets/BikeLab/Segway/Scenes/Sity.unity

<a name="trafficlightbox"></a>
### 3.9.2 TrafficLightBox
#### Description
The TrafficLightBox class is responsible for controlling the visual state of a traffic light box. It allows setting the color of the traffic light (red, yellow, or green) by changing the materials of the associated mesh renderers.

#### Fields
- **track**: The TrackSpline associated with the traffic light box.
- **inverse**: A boolean flag indicating if the traffic light colors should be inverted.
- **red**: A list of MeshRenderer components representing the red light.
- **yellow**: A list of MeshRenderer components representing the yellow light.
- **green**: A list of MeshRenderer components representing the green light.
- **redOff**: The material to use when the red light is off.
- **redOn**: The material to use when the red light is on.
- **yellowOff**: The material to use when the yellow light is off.
- **yellowOn**: The material to use when the yellow light is on.
- **greenOff**: The material to use when the green light is off.
- **greenOn**: The material to use when the green light is on.

![TrafficLightBox](https://github.com/V-Kudryashov/md-Files/assets/17885189/a7e5be29-018c-47ff-be97-39282968eec2)

#### Public Methods
- **setColor(TrafficColor color)**: Sets the color of the traffic light box. Accepts a TrafficColor enum value (Red, Yellow, or Green) and updates the materials of the mesh renderers accordingly.

<a name="speedlimits"></a>
### 3.10 SpeedLimits
#### Description
The SpeedLimits script manages the placement and removal of speed limit road signs along a track. It calculates the maximum speed for each turn based on the turn radius and displays this information on the road signs.

![SpeedLimits](https://github.com/V-Kudryashov/md-Files/assets/17885189/7e100bae-47da-4e53-9426-5afc6f981f48)

#### Fields
- **prefab**: The road sign prefab to be instantiated.
- **trackSpline**: The TrackSpline object representing the track.
#### Buttons
- **Add Road Signs**: Adds road signs along the track at each turn, displaying the maximum speed for that turn.
- **Remove Road Signs**: Removes all road sign game objects with the same tag as the prefab.

<a name="motoball"></a>
# 4 Motoball

<a name="motoballcontroller"></a>
## 4.1 MotoballController

### Description
The MotoballController script is responsible for controlling the behavior of a motoball bike. It handles the movement of the bike towards the ball, holding the ball, movement to the goal and hitting the ball.

![MotoballController](https://github.com/V-Kudryashov/md-Files/assets/17885189/92438dad-d15b-4d23-9079-c53ce3e43817)

### Fields
- **ball**: The transform representing the ball in the scene.
- **goal**: The transform representing the goal in the scene.
- **wall**: The transform representing the wall in the scene.
- **target**: The target transform towards which the bike should move.
- **approachDirection**: Direction of approach to the ball.
- **zSpeedValue**: Value for incline dumper interpolation between rotation speed and rotation value.
- **zDumper**: Damping factor for z-axis rotation.
- **minGoalTangent**: The bike moves towards the goal along a Bezier curve. The minGoalTangent field specifies the minimum length of the p3p2 vector. The longer the length, the smoother the bike approaches the goal. A vector that is too long increases the path length.
- **minBallTangent**: The bike moves towards the ball along a Bezier curve. The minGoalTangent field specifies the minimum length of the p3p2 vector. The longer the length, the smoother the bike approaches the ball. A vector that is too long increases the path length.
### Public methods
- **start()**: Starts the bike's movement.
- **reset()**: Resets the bike, biker and ball;

<a name="segway"></a>
# 5 Segway

<a name="segwayscript"></a>
## 5.1 Segway

### Description
The Segway script controls the behavior of a Segway-like vehicle. It uses a pair of [HingeJoint](https://docs.unity3d.com/2023.2/Documentation/Manual/class-HingeJoint.html) components to simulate the movement of the vehicle's wheels, allowing for realistic physics-based motion.

### Fields
- **jointL**: HingeJoint component for the left wheel.
- **jointR**: HingeJoint component for the right wheel.
- **centerOfMass**: Transform representing the  overall center of mass. Used for visualization during debugging.
- **bodyCenterOfMass**: Transform representing the center of mass of the body of the character. Used for visualization during debugging.

### Public methods
- **setVelocity(float targetV)**: Sets the target velocity of the vehicle.
- **setSideIncline(float incline)**: Sets the target side inclination of the vehicle. By changing the incline you can control the direction of movement.
- **getVelosity()**: Returns the current velocity of the vehicle.
- **getRigidbody()**: Returns the Rigidbody component of the vehicle.
- **reset()**: Resets the position and orientation of the vehicle.

<a name="segwaytrackcontroller"></a>
## 5.2 SegwayTrackController

### Description
The SegwayTrackController class is an implementation of the TrackController class. The Segway script should be attached as the IVehicle.

### Fields
- **waitStart**: Boolean flag indicating whether the vehicle should stay.

### Public methods
- **getL()**: Returns the current position of the vehicle on the track.

<a name="segwayik"></a>
## 5.4 SegwayIK

### Description
The SegwayIK script implements inverse kinematics (IK) for a Segway driver. It adjusts the position and rotation of the character's body parts (hands, feet, hips, and spine) to match the movement of the vehicle and simulate realistic behavior.

### Fields
- **leftHandHandle**: Transform representing the left hand IK target.
- **rightHandHandle**: Transform representing the right hand IK target.
- **leftFootHandle**: Transform representing the left foot IK target.
- **rightFootHandle**: Transform representing the right foot IK target.
- **frame1**: Transform representing the base of the segway.
- **frame2**: Transform representing the segway handle.
- **segway**: SegwayController component representing the Segway vehicle controller.
- **forwardInclineThreshold**: Threshold for forward incline angle. If the incline is less than this threshold, the character does not react.

<a name="examples"></a>
# 6 Examples
<a name="bicycle"></a>
## 6.1 Bicycle
### Description
The Bicycle scene demonstrates the use of the Bicycle prefab. This is a minimalist scene. The bike is controlled manually.

![Bicycle](https://github.com/V-Kudryashov/md-Files/assets/17885189/2482571d-143f-4900-a036-5133c4e8ee09)

<a name="bikeexample"></a>
## 6.2 Bike
### Description
The Bike scene demonstrates the use of a basic Bike Lab asset script, the BikeController script.

![Bike](https://github.com/V-Kudryashov/md-Files/assets/17885189/b34a2138-33ec-40ba-8194-fcadba9b180a)

<a name="demo"></a>
## 6.3 Demo
### Description
The Demo scene lets you see all examples.

![Demo](https://github.com/V-Kudryashov/md-Files/assets/17885189/a292eeec-b87c-4b63-996a-1cd92aba164c)

<a name="motocross"></a>
## 6.4 Motocross
This section provides examples of using the CrossBike prefab.
<a name="bigJump"></a>
### 6.4.1 BigJump

![BigJump](https://github.com/V-Kudryashov/md-Files/assets/17885189/a2ee0c6c-0361-4451-ab3d-21da1b9080c8)

In the BigJump  scene CrossBike makes a 150 m long jump. In order to successfully complete such a long jump, special measures had to be taken.
- The profile of the springboard was made smooth enough so that the shock absorbers do not press in completely and so that the front wheel does not go down.
- FixedDeltaTime reduced by 10 times (PhysicSattings script).
<a name="motocrossscene"></a>
### 6.4.2 Motocross

![Motocross](https://github.com/V-Kudryashov/md-Files/assets/17885189/bc4be341-ad22-468c-927b-ec4c339b2d42)

The Motocross scene demonstrates the use of an additional spline. Using the TreckSpline script, a track close to the original was laid. Then, the TreckSpline2 script was added and an additional spline was generated. The second spline is smoother than the first and the bikes can go noticeably faster.
<a name="slow"></a>
### 6.4.3 Slow

![Slow](https://github.com/V-Kudryashov/md-Files/assets/17885189/0b5eb30e-7f24-4176-90aa-79e320a33b84)

In the Slow scene Track  it is laid out in the form of a small figure eight.

<a name="roadracing"></a>
## 6.5 Road Racing
This section provides examples of using the RoadBike prefab.
<a name="donington"></a>
### 6.5.1 Donington

![Donington](https://github.com/V-Kudryashov/md-Files/assets/17885189/b80604de-aee1-4289-92d0-e5441398f77d)

The 'Donington' scene is an example of a flat track.
<a name="eight"></a>
### 6.5.2 Eight

![Eight](https://github.com/V-Kudryashov/md-Files/assets/17885189/a243a84d-9ed2-400e-ad85-db305696e732)

In this scene, the Track is laid out in the form of a three-dimensional figure eight.<a name="slow"></a>
<a name="suzuka"></a>
### 6.5.3 Suzuka

![Suzuka](https://github.com/V-Kudryashov/md-Files/assets/17885189/34292ff2-7b98-432d-a88d-eb7ab1bf3bf0)

The Suzuka scene is a model of a real track. 
