using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterSim.ClusterLib.Calculation.Gpu
{
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.Remoting.Contexts;

    using ClusterSim.ClusterLib.Calculation.OpenCl;

    using OpenCL.Net.Extensions;
    using OpenCL.Net;


    public static class ComputeWorker
    {
        private static ErrorCode err;

        private static Event event0;

        private static Program program;

        private static Mem memPosMass;

        private static Mem memPosMassNew;

        private static Mem memVel;

        private static Mem memAcc;

        private static Kernel kernel;

        private static int count;

        private static bool inverted;
        
        private static Platform[] Platforms { get; } = Cl.GetPlatformIDs(out err);

        private static Device[] Devices { get; } = Cl.GetDeviceIDs(Platforms[0], DeviceType.Gpu, out err);

        private static Device Device { get; } = Devices[0]; //cl_device_id device;

        private static OpenCL.Net.Context Context { get; } =
            Cl.CreateContext(null, 1, Devices, null, IntPtr.Zero, out err);

        private static CommandQueue CmdQueue { get; set; } = Cl.CreateCommandQueue(
            Context,
            Device,
            CommandQueueProperties.None,
            out err);

        public static void Initialize(int count)
        {
            // Create and build a program from our OpenCL-C source code
            string programSource;
            try
            {
                programSource = File.ReadAllText(
                    @"C:\Users\Dennis\Source\Repos\ClusterSim\Clusterlib\Calculation\Gpu\RK4.cl");
            }
            catch
            {
                programSource = File.ReadAllText(@"C:\Users\Dennis\Desktop\ClusterSim\Clusterlib\Calculation\Gpu\RK4.cl");
            }

            program = Cl.CreateProgramWithSource(Context, 1, new[] { programSource }, null, out err);
            Cl.BuildProgram(program, 0, null, "", null, IntPtr.Zero); //" - cl-mad-enable"
            
            // Check for any compilation errors
            if (Cl.GetProgramBuildInfo(program, Device, ProgramBuildInfo.Status, out err).CastTo<BuildStatus>()
                != BuildStatus.Success)
            {
                if (err != ErrorCode.Success)
                    Console.WriteLine("ERROR: " + "Cl.GetProgramBuildInfo" + " (" + err.ToString() + ")");
                Console.WriteLine("Cl.GetProgramBuildInfo != Success");
                Console.WriteLine(Cl.GetProgramBuildInfo(program, Device, ProgramBuildInfo.Log, out err));
            }

            // Create a kernel from our program
            kernel = Cl.CreateKernel(program, "calcAcc", out err);

            memPosMass = (Mem)Cl.CreateBuffer(Context, MemFlags.ReadWrite, sizeof(double) * 4 * count, out err);
            memPosMassNew = (Mem)Cl.CreateBuffer(Context, MemFlags.ReadWrite, sizeof(double) * 4 * count, out err);
            memVel = (Mem)Cl.CreateBuffer(Context, MemFlags.ReadWrite, sizeof(double) * 4 * count, out err);
            memAcc = (Mem)Cl.CreateBuffer(Context, MemFlags.WriteOnly, sizeof(double) * 4 * count, out err);
            
            // Get the maximum number of work items supported for this kernel on this device
            IntPtr notused;
            InfoBuffer local = new InfoBuffer(new IntPtr(4));
            var info = Cl.GetKernelWorkGroupInfo(
                kernel,
                Device,
                KernelWorkGroupInfo.WorkGroupSize,
                new IntPtr(sizeof(int)*4),
                local,
                out notused);

            // Set the arguments to our kernel, and enqueue it for execution
            //kernel.;
            Cl.SetKernelArg(kernel, 2, new IntPtr(IntPtr.Size), memVel);
            Cl.SetKernelArg(kernel, 3, new IntPtr(IntPtr.Size), memAcc);
            Cl.SetKernelArg(kernel, 7, new IntPtr(sizeof(double) * 4 * count), null);
            ComputeWorker.count = count;
        }
        

        public static void DoStep(Star[] stars, double dt, int burst, double distance)
        {
            Cl.EnqueueWriteBuffer(CmdQueue, memPosMass, Bool.True, IntPtr.Zero, new IntPtr(sizeof(double) * 4 * count), stars.Select(x => x.GetDouble4()).ToArray(), 0, null, out event0);

            Cl.EnqueueWriteBuffer(CmdQueue, memVel, Bool.True, IntPtr.Zero, new IntPtr(sizeof(double) * 4 * count), stars.Select(x => x.GetVel4()).ToArray(), 0, null, out event0);
            
            Cl.SetKernelArg(kernel, 4, new IntPtr(sizeof(double)), dt);
            Cl.SetKernelArg(kernel, 5, new IntPtr(sizeof(double)), distance);
            Cl.SetKernelArg(kernel, 6, new IntPtr(sizeof(double)), Cluster.Cluster.GalaxyMass);

            var GlobalWorkSizePtr = new[] { new IntPtr(count) };

            //burst = 2;
            for (int i = 0; i < burst; i += 2)
            {
                Cl.SetKernelArg(kernel, 0, new IntPtr(IntPtr.Size), memPosMass);
                Cl.SetKernelArg(kernel, 1, new IntPtr(IntPtr.Size), memPosMassNew);
                Cl.EnqueueNDRangeKernel(CmdQueue, kernel, 1, null, GlobalWorkSizePtr, null, 0, null, out event0);
                //Cl.Finish(CmdQueue);

                Cl.SetKernelArg(kernel, 0, new IntPtr(IntPtr.Size), memPosMassNew);
                Cl.SetKernelArg(kernel, 1, new IntPtr(IntPtr.Size), memPosMass);
                Cl.EnqueueNDRangeKernel(CmdQueue, kernel, 1, null, GlobalWorkSizePtr, null, 0, null, out event0);
                //Cl.Finish(CmdQueue);
            }

            // Force the command queue to get processed, wait until all commands are complete
            Cl.Finish(CmdQueue);


            // Read back the results
            double4[] acceleration = new double4[count];
            double4[] positions = new double4[count];
            double4[] velocitys = new double4[count];


            Cl.EnqueueReadBuffer(CmdQueue, (IMem)memAcc, Bool.True, IntPtr.Zero, new IntPtr(count * 4 * sizeof(double)), acceleration, 0, null, out event0);
            Cl.EnqueueReadBuffer(CmdQueue, memPosMass, Bool.True, IntPtr.Zero, new IntPtr(count * 4 * sizeof(double)), positions, 0, null, out event0); /// ändern
            Cl.EnqueueReadBuffer(CmdQueue, (IMem)memVel, Bool.True, IntPtr.Zero, new IntPtr(count * 4 * sizeof(double)), velocitys, 0, null, out event0);

            for (int i = 0; i < count; i++)
            {
                stars[i].Load(positions[i], velocitys[i], acceleration[i], dt);
            }
            

            if (stars.Any(x => x.Pos.IsNull()))
                {
                throw new Exception("Fehler" );
            }
        }

        private static void setArgs(double dt)
        {
            double a = dt;
            if (inverted)
            {
                var error = Cl.SetKernelArg(kernel, 0, new IntPtr(4), memPosMassNew);
                error = Cl.SetKernelArg(kernel, 1, new IntPtr(4), memPosMass);
            }
            else
            {
                var error = Cl.SetKernelArg(kernel, 0, new IntPtr(4), memPosMass);
                error = Cl.SetKernelArg(kernel, 1, new IntPtr(4), memPosMassNew);
            }

            Cl.SetKernelArg(kernel, 4, new IntPtr(sizeof(double)), a);
            inverted = !inverted;
        }
    }
}
