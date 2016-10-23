using System.Threading.Tasks;
using Urho;
using Urho.Gui;
using Urho.Actions;
using System;

namespace MyGameInfrastructure
{
    public class MyGameInfrastructure : Application
	{
		const string CoinstFormat = "{0} coins";

		Scene _scene;
        private Camera _camera;
        private Vector3 _cameraDirection;
        private Text _text;
        //private bool _isMouseDown = false;
        private TaskCompletionSource<bool> _liveTask;
        private InfantryPlatoon[] _infantryPlatoons = new InfantryPlatoon[3];

		public Viewport Viewport { get; private set; }

		public MyGameInfrastructure() 
            : base(new ApplicationOptions(assetsFolder: "Data")
                {
                    Height = 736,
                    Width = 414,
                    Orientation = ApplicationOptions.OrientationType.Portrait
                }
            )
        {
        }

		protected override void Start()
		{
			base.Start();
            Input.SubscribeToKeyDown(KeyDownHandler);
            CreateScene();
        }

        private void KeyDownHandler(KeyDownEventArgs e)
        {
            switch (e.Key)
            {
                case Key.W:
                    AdjustCameraPosition(new Vector3(0, 0, 10));
                    break;
                case Key.S:
                    AdjustCameraPosition(new Vector3(0, 0, -10));
                    break;
                case Key.Esc:
                    Exit();
                    break;
            }
        }

        private void AdjustCameraPosition(Vector3 deltaPosition)
        {
            //_camera.Node.Position += deltaPosition;
            //cameraNode.GetComponent()

            //Viewport.Camera.Node.Position += deltaPosition;
            //Renderer.SetViewport(0, Viewport);
        }

        private InfantryPlatoon CreateInfantryUnit(float x, float z)
        {
            InfantryPlatoon retVal = new InfantryPlatoon();

            float spacer = 0.1f;
            for (int curRank = 0; curRank < 3; curRank++)
            {
                for (int curFile = 0; curFile < 12; curFile++)
                {
                    var xPos = x + curFile + curFile * spacer;
                    var zPos = z + curRank + curRank * spacer;
                    var position = new Vector3(xPos, 0, zPos);

                    var nodeAndModel = CreateStandardPlane(position, 1.0f, "Materials/infantryStanding.xml");

                    retVal.InfantryMen[curRank, curFile] = new InfantryMan { Node = nodeAndModel.Item1, Model = nodeAndModel.Item2 };
                }
            }

            return retVal;
        }

        async void CreateScene()
		{
			_scene = new Scene();
			_scene.CreateComponent<Octree>();

            _infantryPlatoons[0] = CreateInfantryUnit(0, 0);
            _infantryPlatoons[1] = CreateInfantryUnit(17.3f, 0);
            _infantryPlatoons[2] = CreateInfantryUnit(-17.3f, 0);


            //CreateStandardPlane(new Vector3(25, 0, 25), "Materials/MyTextureGreen.xml");
            //CreateBox(new Vector3(0, 0, 0), "Materials/MyTextureGreen.xml");

            SetupDirectionalLight(new Vector3(0, 1000, 0), new Vector3(0, -1, 0), 1.5f);
            SetupDirectionalLight(new Vector3(-1000, 0, 0), new Vector3(1, 0, 0), 0.5f);
            SetupDirectionalLight(new Vector3(0, 0, -1000), new Vector3(0, 0, 1), 0.5f);
            //CreatePointLight();

            //var physics = scene.CreateComponent<PhysicsWorld>();
            //physics.SetGravity(new Vector3(0, 0, 0));

            _text = new Text();
            _text.Value = "This is my text.";
            _text.SetColor(new Color(1, 0, 0, 1));
            _text.HorizontalAlignment = HorizontalAlignment.Left;
            _text.SetFont(ResourceCache.GetFont("Fonts/Font.ttf"), Graphics.Width / 40);
            UI.Root.AddChild(_text);

            SetupCamera();

            Input.SetMouseVisible(true, false);
            
            await StartGameTask();
			
            Exit();
        }

        private void CreateBox(Vector3 position, string materialFileName)
        {
            var boxNode = _scene.CreateChild("Box_" + materialFileName);

            boxNode.Scale = new Vector3(100, 100, 100);
            boxNode.Position = position;

            var boxComponent = boxNode.CreateComponent<StaticModel>();
            boxComponent.Model = ResourceCache.GetModel("Models/box.mdl");
            boxComponent.SetMaterial(ResourceCache.GetMaterial(materialFileName));

        }

        private Tuple<Node, StaticModel> CreateStandardPlane(Vector3 position, float scale, string materialFileName)
        {
            var planeNode = _scene.CreateChild("Plane_" + materialFileName);
            planeNode.Scale = new Vector3(scale, 1, (32.0f / 54.0f)*scale);
            planeNode.Position = position;
            var planeObject = planeNode.CreateComponent<StaticModel>();
            planeObject.Model = ResourceCache.GetModel("Models/Plane.mdl");
            planeObject.SetMaterial(ResourceCache.GetMaterial(materialFileName));
            
            return new Tuple<Node, StaticModel>(planeNode, planeObject);
        }

        private void CreatePointLight()
        {
            var pointLightNode = _scene.CreateChild("PointLight");
            pointLightNode.Position = new Vector3(50, 2, 50);
            //pointLightNode.SetDirection(new Vector3(0, -1, 0));
            var pointLightComponent = pointLightNode.CreateComponent<Light>();
            pointLightComponent.Brightness = 2.0F;
            pointLightComponent.Color = Color.White;
            pointLightComponent.LightType = LightType.Point;
            pointLightComponent.FadeDistance = 5;
            pointLightComponent.Range = 25;
        }

        private void SetupCamera()
        {
            var cameraNode = _scene.CreateChild("camera");
            var _camera = cameraNode.CreateComponent<Camera>();

            cameraNode.Position = (new Vector3(0.0f, 100.0f, 0.0f));
            _cameraDirection = new Vector3(0.0f, -1, 0.0f);

            //cameraNode.Position = (new Vector3(-200.0f, 0.0f, 0.0f));
            //_cameraDirection = new Vector3(1.0f, 0, 0.0f);

            _cameraDirection.Normalize();
            cameraNode.SetDirection(_cameraDirection);

            Renderer.SetViewport(0, Viewport = new Viewport(Context, _scene, _camera, null));
        }
        
        private void SetupDirectionalLight(Vector3 position, Vector3 direction, float brightness)
        {
            var directionalLightNode = _scene.CreateChild(string.Format("DirLight_{0}_{1}_{2}", position.X, position.Y, position.Z));
            directionalLightNode.Position = position;
            directionalLightNode.SetDirection(direction);
            var directionalLightComponent = directionalLightNode.CreateComponent<Light>();
            directionalLightComponent.Brightness = brightness;
            directionalLightComponent.LightType = LightType.Directional;
        }

        private Task StartGameTask()
        {
            _liveTask = new TaskCompletionSource<bool>();
            return _liveTask.Task;
        }

        private void SetCameraPosition()
        {
            const float speed = 0.5f;

            var headingAngle = MyMath.Atan2(_cameraDirection.Z, _cameraDirection.X);
            var verticalAngle = MyMath.Atan2(_cameraDirection.Y, 1);
            var zDir = MyMath.Sin(headingAngle);
            var xDir = MyMath.Cos(headingAngle);
            var cameraPositionDelta = new Vector3();
            if (Input.GetKeyDown(Key.W))
            {
                cameraPositionDelta.X += speed * xDir;
                cameraPositionDelta.Z += speed * zDir;
            }


            if (Input.GetKeyDown(Key.S))
            {
                cameraPositionDelta.X -= speed * xDir;
                cameraPositionDelta.Z -= speed * zDir;
            }

            if (Input.GetKeyDown(Key.A))
            {
                cameraPositionDelta.X -= speed * zDir;
                cameraPositionDelta.Z += speed * xDir;
            }

            if (Input.GetKeyDown(Key.D))
            {
                cameraPositionDelta.X += speed * zDir;
                cameraPositionDelta.Z -= speed * xDir;
            }

            if (Input.GetKeyDown(Key.N1))
            {
                cameraPositionDelta.Y += speed;
            }

            if (Input.GetKeyDown(Key.N2))
            {
                cameraPositionDelta.Y -= speed;
            }
            Viewport.Camera.Node.Position += cameraPositionDelta;

        }

        private void SetCameraRotation()
        {
            const float rotationSpeed = 0.15f;
            float leftRightRotationDelta = 0.0f;
            float upDownRotationDelta = 0.0f;

            if (Input.GetKeyDown(Key.Left))
                leftRightRotationDelta -= rotationSpeed;
            if (Input.GetKeyDown(Key.Right))
                leftRightRotationDelta += rotationSpeed;

            if (Input.GetKeyDown(Key.Up))
                upDownRotationDelta -= rotationSpeed;
            if (Input.GetKeyDown(Key.Down))
                upDownRotationDelta += rotationSpeed;

            var rotationQuaternion = Viewport.Camera.Node.Rotation;
            var originalPitchAngle = rotationQuaternion.PitchAngle;

            //pitch must be "zeroed out" before we yaw
            Viewport.Camera.Node.Pitch(-originalPitchAngle, TransformSpace.Local);

            Viewport.Camera.Node.Roll(leftRightRotationDelta, TransformSpace.Local);

            //now set pitch back to its original angle before applying rotation delta for this frame
            Viewport.Camera.Node.Pitch(originalPitchAngle, TransformSpace.Local);
            Viewport.Camera.Node.Pitch(upDownRotationDelta, TransformSpace.Local);

            _cameraDirection = Viewport.Camera.Node.Direction;
        }

        private void DisplayCameraRotationInfo()
        {
            var rotationQuaternion = Viewport.Camera.Node.Rotation;
            _text.Value = string.Format("Yaw: {1}{0}Pitch: {2}{0}Roll: {3}", Environment.NewLine, rotationQuaternion.YawAngle, rotationQuaternion.PitchAngle, rotationQuaternion.RollAngle);
            _text.Value += Environment.NewLine + Environment.NewLine;
            _text.Value += string.Format("X:{1}{0}Y:{2}{0}Z:{3}", Environment.NewLine, rotationQuaternion.X, rotationQuaternion.Y, rotationQuaternion.Z);
        }

        protected override void OnUpdate(float timeStep)
        {
            if (Input.GetMouseButtonDown(MouseButton.Left) || Input.NumTouches > 0)
            {
                _liveTask.SetResult(true);
            }

            SetCameraPosition();
            SetCameraRotation();
            DisplayCameraRotationInfo();
            Renderer.SetViewport(0, Viewport);

            if (Input.GetKeyDown(Key.Space))
            {
                var guyToDieIndex = (new Random()).Next(12);
                if(!_infantryPlatoons[0].InfantryMen[2, guyToDieIndex].IsDead)
                {
                    _infantryPlatoons[0].InfantryMen[2, guyToDieIndex].Model.Remove();
                    _infantryPlatoons[0].InfantryMen[2, guyToDieIndex].Node.Remove();
                    _infantryPlatoons[0].InfantryMen[2, guyToDieIndex].IsDead = true;
                }
            }

            base.OnUpdate(timeStep);
        }
    }
}
