using System;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Azure.Kinect.BodyTracking;
using System.IO;
using System.Text;
using WiimoteLib;
using System.Timers;

namespace KinectCSV
{
    class Program
    {
        private static Wiimote wiimote = new Wiimote();

        static void Main(string[] args)
        {
            var sw = new System.Diagnostics.Stopwatch();
            wiimote.Connect();
            Console.WriteLine("ESC:STOP");
            String now = DateTime.Now.ToString("MMddHHmmss");
            Console.WriteLine("Start Body Tracking App!");
            
            //接続されている機器の数をチェック
            if (Device.GetInstalledCount() == 0)
            {
                Console.WriteLine("No k4a devices attached!");
                Console.ReadKey();
                return;
            }

            //■デバイスを開いてカメラを起動する
            Device device = null;
            // Open the first plugged in Kinect device
            try
            {
                //1台目に接続
                device = Device.Open(0);
            }
            catch (AzureKinectOpenDeviceException ex)
            {
                Console.WriteLine("Failed to open k4a device!!");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace.ToString());
                Console.ReadKey();
                return;
            }
            // Start camera. Make sure depth camera is enabled.
            var deviceConfig = new DeviceConfiguration();
            deviceConfig.DepthMode = DepthMode.NFOV_Unbinned;
            deviceConfig.ColorResolution = ColorResolution.Off;
            try
            {
                device.StartCameras(deviceConfig);
            }
            catch (AzureKinectStartCamerasException ex)
            {
                Console.WriteLine("Failed to open k4a device!!");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace.ToString());
                device.Dispose();
                Console.ReadKey();
                return;
            }
            using (StreamWriter file = new StreamWriter(@"C:/Users/hackathon/Desktop/" + now + ".csv", true))
            {
                file.Write("Time[ms],PELVIS_X,PELVIS_Y,PELVIS_Z," +
                                            "SPINE_NAVAL_X,SPINE_NAVAL_Y,SPINE_NAVAL_Z," +
                                            "SPINE_CHEST_X,SPINE_CHEST_Y,SPINE_CHEST_Z," +
                                            "NECK_X,NECK_Y,NECK_Z," +
                                            "CLAVICLE_LEFT_X,CLAVICLE_LEFT_Y,CLAVICLE_LEFT_Z," +
                                            "SHOULDER_LEFT_X,SHOULDER_LEFT_Y,SHOULDER_LEFT_Z," +
                                            "ELBOW_LEFT_X,ELBOW_LEFT_Y,ELBOW_LEFT_Z," +
                                            "WRIST_LEFT_X,WRIST_LEFT_Y,WRIST_LEFT_Z," +
                                            "HAND_LEFT_X,HAND_LEFT_Y,HAND_LEFT_Z," +
                                            "HANDTIP_LEFT_X,HANDTIP_LEFT_Y,HANDTIP_LEFT_Z," +
                                            "THUMB_LEFT_X,THUMB_LEFT_Y,THUMB_LEFT_Z," +
                                            "CLAVICLE_RIGHT_X,CLAVICLE_RIGHT_Y,CLAVICLE_RIGHT_Z," +
                                            "SHOULDER_RIGHT_X,SHOULDER_RIGHT_Y,SHOULDER_RIGHT_Z," +
                                            "ELBOW_RIGHT_X,ELBOW_RIGHT_Y,ELBOW_RIGHT_Z," +
                                            "WRIST_RIGHT_X,WRIST_RIGHT_Y,WRIST_RIGHT_Z," +
                                            "HAND_RIGHT_X,HAND_RIGHT_Y,HAND_RIGHT_Z," +
                                            "HANDTIP_RIGHT_X,HANDTIP_RIGHT_Y,HANDTIP_RIGHT_Z," +
                                            "THUMB_RIGHT_X,THUMB_RIGHT_Y,THUMB_RIGHT_Z," +
                                            "HIP_LEFT_X,HIP_LEFT_Y,HIP_LEFT_Z," +
                                            "KNEE_LEFT_X,KNEE_LEFT_Y,KNEE_LEFT_Z," +
                                            "ANKLE_LEFT_X,ANKLE_LEFT_Y,ANKLE_LEFT_Z," +
                                            "FOOT_LEFT_X,FOOT_LEFT_Y,FOOT_LEFT_Z," +
                                            "HIP_RIGHT_X,HIP_RIGHT_Y,HIP_RIGHT_Z," +
                                            "KNEE_RIGHT_X,KNEE_RIGHT_Y,KNEE_RIGHT_Z," +
                                            "ANKLE_RIGHT_X,ANKLE_RIGHT_Y,ANKLE_RIGHT_Z," +
                                            "FOOT_RIGHT_X,FOOT_RIGHT_Y,FOOT_RIGHT_Z," +
                                            "HEAD_X,HEAD_Y,HEAD_Z," +
                                            "NOSE_X,NOSE_Y,NOSE_Z," +
                                            "EYE_LEFT_X,EYE_LEFT_Y,EYE_LEFT_Z," +
                                            "EAR_LEFT_X,EAR_LEFT_Y,EAR_LEFT_Z," +
                                            "EYE_RIGHT_X,EYE_RIGHT_Y,EYE_RIGHT_Z," +
                                            "EAR_RIGHT_X,EAR_RIGHT_Y,EAR_RIGHT_Z," +
                                            "Weight(kg),B_TopLeft(kg),B_TopRight(kg),B_UnderLeft(kg),B_UnderRight(kg),B_Center_X(cm),B_Center_Y(cm)" +
                                            "\n");
            }


            //■トラッカーを作成する
            var calibration = device.GetCalibration(deviceConfig.DepthMode, deviceConfig.ColorResolution);
            var trackerConfig = new TrackerConfiguration();
            trackerConfig.ProcessingMode = TrackerProcessingMode.Gpu;       //GPUがない場合はCpuを指定
            trackerConfig.SensorOrientation = SensorOrientation.Default;
            using (var tracker = Tracker.Create(calibration, trackerConfig))
            {
                var wantExit = false;

                while (!wantExit)
                {

                    //■Azure Kinect デバイスからキャプチャを取得する
                    // Capture a depth frame
                    using (Capture sensorCapture = device.GetCapture())
                    {
                        // Queue latest frame from the sensor.
                        tracker.EnqueueCapture(sensorCapture);
                    }

                    // Try getting latest tracker frame.
                    using (Frame frame = tracker.PopResult(TimeSpan.Zero, throwOnTimeout: false))
                    {

                        if (frame != null)
                        {
                            if (frame.NumberOfBodies > 0)
                            {
                                var skeleton = frame.GetBodySkeleton(0);
                                BalanceBoardState bbs = wiimote.WiimoteState.BalanceBoardState;
                                using (StreamWriter file = new StreamWriter(@"C:/Users/hackathon/Desktop/" + now + ".csv", true))
                                {
                                    sw.Start();
                                    file.Write(sw.ElapsedMilliseconds + ",");
                                    for (int i = 0; i < 32; i += 1)
                                    {
                                        file.Write(skeleton.GetJoint(i).Position.X + "," + skeleton.GetJoint(i).Position.Y + "," + skeleton.GetJoint(i).Position.Z + ",");
                                    }

                                    file.Write(bbs.WeightKg + "," + bbs.SensorValuesKg.TopLeft / 4 + "," + bbs.SensorValuesKg.TopRight / 4 + "," + bbs.SensorValuesKg.BottomLeft / 4 + "," + bbs.SensorValuesKg.BottomRight / 4 + "," + bbs.CenterOfGravity.X + "," + bbs.CenterOfGravity.Y);
                                    file.Write("\n");
                                    Console.WriteLine(bbs.WeightKg + "[kg]");

                                }
                            }
                        }
                    }
                    //Escキーで終了
                    if (Console.KeyAvailable)
                    {
                        var outChar = Console.ReadKey().Key.ToString();
                        if (outChar == "Escape")
                        {
                            return;
                        }
                    }
                }
            }
            device.StopCameras();
            device.Dispose();
        }
    }
    //ref(https://devlog.arksystems.co.jp/2020/07/14/13673/)
}