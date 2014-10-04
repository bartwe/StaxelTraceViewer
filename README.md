StaxelTraceViewer
=================

Logger and visualizer that can be used to investigate where a program is spending time, especially suited to gameloops.

```csharp
        void Example() {
            if (File.Exists("staxeltrace.on"))
                TraceRecorder.Start();

            using (new TraceScope(TraceKeys.WorldRenderer_RenderChunks)) {
                // timed work
                
            }

            TraceRecorder.Flush(); // periodically call to write to disk, 

            using (new TraceScope(TraceKeys.WorldRenderer_Cleanup)) {
                // timed work

            }

            TraceRecorder.Stop();
        }
```
