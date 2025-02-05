using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VK.BikeLab
{
    public enum TrafficColor { Red = 0, Yellow = 1, Green = 2 }
    [HelpURL("https://github.com/V-Kudryashov/md-Files/blob/main/BikeLab/BikeLab.md#371-trafficlight")]
    public class TrafficLight : MonoBehaviour
    {
        public Direction dir1;
        public Direction dir2;
        public List<Track> tracks;
        [Header("Info")]
        public TrafficColor color1;
        public TrafficColor color2;

        public enum Phases { Yellow1, Red, Yellow2, Green}
        private TrafficLightBox[] boxes;
        private Phases phase;
        private float phaseEnd;
        void Start()
        {
            boxes = GetComponentsInChildren<TrafficLightBox>();

            initTracks();

            setColor(TrafficColor.Yellow);
            phase = Phases.Yellow1;
            phaseEnd = Time.time + dir2.brakeTime;
        }
        private void FixedUpdate()
        {
            autoSet();
        }
        private void initTracks()
        {
            foreach (Track track in tracks)
            {
                Vector3 pos = track.track.transform.InverseTransformPoint(transform.position);
                track.track.spline.getClosestL(pos, out float s);
                track.s = s;

                if (track.dir1)
                    track.stopLine = dir1.stopLine;
                else
                    track.stopLine = dir2.stopLine;
            }
        }
        private void autoSet()
        {
            if (Time.time >= phaseEnd)
            {
                switch (phase)
                {
                    case Phases.Yellow1:
                        {
                            setColor(TrafficColor.Green);
                            phase = Phases.Green;
                            phaseEnd = Time.time + dir1.greenTime;
                            break;
                        }
                    case Phases.Green:
                        {
                            setColor(TrafficColor.Yellow);
                            phase = Phases.Yellow2;
                            phaseEnd = Time.time + dir1.brakeTime;
                            break;
                        }
                    case Phases.Yellow2:
                        {
                            setColor(TrafficColor.Red);
                            phase = Phases.Red;
                            phaseEnd = Time.time + dir2.greenTime;
                            break;
                        }
                    case Phases.Red:
                        {
                            setColor(TrafficColor.Yellow);
                            phase = Phases.Yellow1;
                            phaseEnd = Time.time + dir2.brakeTime;
                            break;
                        }
                }
            }
        }
        private void setColor(TrafficColor color)
        {
            foreach (TrafficLightBox box in boxes)
                box.setColor(color);
            
            color1 = color;
            switch (color)
            {
                case TrafficColor.Red: { color2 = TrafficColor.Green; break; }
                case TrafficColor.Green: { color2 = TrafficColor.Red; break; }
                case TrafficColor.Yellow: { color2 = TrafficColor.Yellow; break; }
            }

            foreach (Track track in tracks)
                if (track.dir1)
                    track.trafficColor = color1;
                else
                    track.trafficColor = color2;
        }
        [Serializable]
        public class Direction
        {
            public float greenTime = 5;
            public float brakeTime = 2;
            public float stopLine = 10;
        }
        [Serializable]
        public class Track
        {
            public TrackSpline track;
            public bool dir1;
            [Header("Info")]
            public float s;
            public float stopLine;
            public TrafficColor trafficColor;
        }
    }
}