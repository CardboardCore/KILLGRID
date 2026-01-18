using Attic.Cameras.Modules;
using Attic.Utilities;

namespace Run.Cameras
{
    public class StepForwardBackwardModule : CameraModule
    {

        protected override void OnStart()
        {

        }

        protected override void OnStop()
        {

        }

        public void StepForward()
        {
            Log.Write("Step Forward");
        }

        public void StepBackward()
        {
            Log.Write("Step Backward");
        }
    }
}
