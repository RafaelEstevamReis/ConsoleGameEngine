﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using Simple.CGE.Interfaces;

namespace Simple.CGE
{
    public class CGEngine
    {
        public bool Running { get; private set; }
        public bool Paused { get; set; }

        #region Statistics
        public int TargetDrawFps { get; set; } = 30;
        public TimeSpan TargetFrametime
        {
            get
            {
                if (TargetDrawFps == 0) return TimeSpan.Zero;
                return TimeSpan.FromMilliseconds(1_000.0 / TargetDrawFps);
            }
        }

        public TimeSpan LastTotalFrameTime { get; private set; }
        public TimeSpan LastRawFrameTime { get; private set; }
        public double CurrentFPS
        {
            get
            {
                if (LastTotalFrameTime.TotalMilliseconds == 0) return 0;
                return 1000.0 / LastTotalFrameTime.TotalMilliseconds;
            }
        }

        private ulong totalFrames; // 1bi years at 10_000fps
        public ulong TotalFrames => totalFrames;
        public TimeSpan TotalGameTime { get; private set; }
        #endregion

        public event EventHandler<FrameData> OnPreFrame;
        public event EventHandler<FrameData> OnPosFrame;
        public event EventHandler OnSetup;
        public event EventHandler OnStart;

        List<IEntity> EntitiesList { get; }

        public IDrawEngine DrawEngine { get; set; }
        public bool ShowDataOnTitle { get; set; }

        public void AddEntities(params IEntity[] entitiesToAdd) => AddEntities((IEnumerable<IEntity>)entitiesToAdd);
        public void AddEntities(IEnumerable<IEntity> entitiesToAdd) 
        {
            lock (EntitiesList) EntitiesList.AddRange(entitiesToAdd);
        }
        public void RemoveEntities(params IEntity[] entitiesToRemove) => RemoveEntities((IEnumerable<IEntity>)entitiesToRemove);
        public void RemoveEntities(IEnumerable<IEntity> entitiesToRemove)
        {
            lock (EntitiesList)
            {
                foreach (var e in entitiesToRemove) EntitiesList.Remove(e);
            }
        }

        public CGEngine()
            : this(new DrawEngines.FastDraw(new Size(100, 100), new Size(6, 6)))
        { }
        public CGEngine(IDrawEngine drawEngine)
        {
            EntitiesList = new List<IEntity>();
            DrawEngine = drawEngine;
        }

        bool setupCompleted = false;
        public void Setup()
        {
            OnSetup?.Invoke(this, EventArgs.Empty);

            DrawEngine.Setup();

            setupCompleted = true;
        }

        public void Run()
        {
            if (!setupCompleted) throw new InvalidOperationException("Setup method was not called or completed");

            Running = true;
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            DateTime gameStart = DateTime.UtcNow;

            OnStart?.Invoke(this, EventArgs.Empty);

            while (Running)
            {
                TotalGameTime = DateTime.UtcNow - gameStart;
                FrameData data = new FrameData()
                {
                    Engine = this,
                    LastFrameTime = LastTotalFrameTime,
                    DrawEngine = DrawEngine,
                    TotalGameTime = TotalGameTime,
                };

                doPreFrame(sw, data);
                doPhysics(data);
                doDraw(data);
                doPosFrame(sw, data);
            }
        }
        public void Stop() => Running = false;

        private void doPreFrame(System.Diagnostics.Stopwatch sw, FrameData data)
        {
            sw.Restart();
            DrawEngine.PreFrame();
            OnPreFrame?.Invoke(this, data);
        }
        private void doPhysics(FrameData data)
        {
            // Detect colisions
            var colisions = EntitiesList.OfType<IColisionable>()
                                        .ToArray(); // complete enumeration
            foreach (var c in colisions)
            {
                if (c.ColidesWithBorders) 
                {
                    // Check with border
                    if (Helpers.EntityHelper.IntersectWithRectanglePerimeters(c.Rectangle, data.DrawEngine.GameBorder))
                    {
                        c.ColidedWithBorder();
                    }
                }

                if (c.ColidesWithOthers)
                {
                    // check with all others
                    var hits = colisions.Where(co => co != c && c.Rectangle.IntersectsWith(co.Rectangle)).ToArray();
                    if (hits.Length > 0)
                    {
                        c.ColidedWith(hits);
                    }
                }
            }

            // Allow physics to run
            var entities = EntitiesList.OfType<IPhysicsable>()
                                       .ToArray(); // complete enumeration
            foreach (var e in entities)
            {
                if (Paused && !e.PhysicsOnPaused) continue;
                e.DoPhysics(data);
            }
        }
        private void doDraw(FrameData data)
        {
            var entities = EntitiesList.OfType<IDrawable>()
                                       .ToArray(); // complete enumeration

            data.DrawEngine.DrawStart(data);

            // Bkg
            data.DrawEngine.StartFrame(data, DrawLayers.Background);
            foreach (var e in entities)
            {
                if (e.Layer != DrawLayers.Background) continue;
                if (Paused && !e.DrawOnPaused) continue;
                e.DoDraw(data);
            }
            data.DrawEngine.EndFrame(data, DrawLayers.Background);

            // fg
            data.DrawEngine.StartFrame(data, DrawLayers.Foreground);
            foreach (var e in entities)
            {
                if (e.Layer != DrawLayers.Foreground) continue;
                if (Paused && !e.DrawOnPaused) continue;
                e.DoDraw(data);
            }
            data.DrawEngine.EndFrame(data, DrawLayers.Foreground);
            // hud
            data.DrawEngine.StartFrame(data, DrawLayers.HUD);
            foreach (var e in entities)
            {
                if (e.Layer != DrawLayers.HUD) continue;
                if (Paused && !e.DrawOnPaused) continue;
                e.DoDraw(data);
            }
            data.DrawEngine.EndFrame(data, DrawLayers.HUD);

            data.DrawEngine.DrawFinish(data);
        }
        private void doPosFrame(System.Diagnostics.Stopwatch sw, FrameData data)
        {
            DrawEngine.PosFrame();
            OnPosFrame?.Invoke(this, data);

            if (ShowDataOnTitle)
            {
                Console.Title = $"ConsoleEngine  GT:{TotalGameTime:hh\\:mm\\:ss} TF:{TotalFrames} FPS: {CurrentFPS:N1}";
            }

            Interlocked.Increment(ref totalFrames);

            // frame time adjust
            LastRawFrameTime = sw.Elapsed;
            if (TargetDrawFps > 0)
            {
                int frameDiff = (int)(TargetFrametime.TotalMilliseconds - LastRawFrameTime.TotalMilliseconds);
                if (frameDiff > 1) Thread.Sleep(frameDiff);
            }
            // end of frame
            LastTotalFrameTime = sw.Elapsed;
        }

    }
}
