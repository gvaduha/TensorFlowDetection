////
//// TensorFlow.cs; Bindings to the TensorFlow C API for .NET
//// 
//// Authors:
////   Miguel de Icaza (miguel@microsoft.com)
////
//// Strongly typed API
//// The API generally takes a TF_Status that defaults to null, if the value is null, on error, this raises an exception, otherwise, the error is returned on the TF_Status.
//// You can use TFStatus.Default for a value to use when you do not want to create the value yourself and are ok reusing the value.
////
//// Guidaance on doing language bindings for Tensorflow:
//// https://www.tensorflow.org/versions/r0.11/how_tos/language_bindings/
////
////

//using System;
//using System.Runtime.InteropServices;
//using TensorFlow;
//using TF_ImportGraphDefOptions = System.IntPtr;

//namespace SharpStresser
//{
//	static partial class NativeBinding
//	{
//		public const string TensorFlowLibrary = "libtensorflow";
//		public const string TensorFlowLibraryGPU = "libtensorflowgpu";

//		internal static string GetStr(this IntPtr x) => Marshal.PtrToStringAnsi(x);
//	}

//	/// <summary>
//	/// Contains TensorFlow fundamental methods and utility functions.
//	/// </summary>
//	public static class TFCore
//	{
//		internal static void Init()
//		{
//			CheckSize();
//		}

//		static void CheckSize()
//		{
//			unsafe
//			{
//				if (sizeof(IntPtr) == 4)
//				{
//					Console.Error.WriteLine(
//						"The TensorFlow native libraries were compiled in 64 bit mode, you must run in 64 bit mode\n" +
//						"With Mono, do that with mono --arch=64 executable.exe, if using an IDE like MonoDevelop,\n" +
//						"Xamarin Studio or Visual Studio for Mac, Build/Compiler settings, make sure that " +
//						"\"Platform Target\" has x64 selected.");
//					throw new Exception();

//				}
//			}
//		}
//	}


//	/// <summary>
//	/// Base class for many TensorFlow data types that provides a common idiom to dispose and
//	/// release resources associated with the native data types.   Generally, you do not need to use this.
//	/// </summary>
//	/// <remarks>
//	/// <para>
//	/// This implements the Dispose pattern in a reusable form for TensorFlow types.
//	/// </para>
//	/// <para>
//	/// Subclasses invoke the constructor with the handle that this will wrap, and must
//	/// override the NativeDispose method (internal) to release the associated resource.
//	/// </para>
//	/// </remarks>
//	public abstract class TFDisposable : IDisposable
//	{
//		/// <summary>
//		/// The handle this TFDisposable owns.
//		/// </summary>
//		protected IntPtr handle;

//		/// <summary>
//		/// Returns the opaque handle to the object that this TFDisposable owns.
//		/// On assignment, the previous handle will be disposed first.
//		/// </summary>
//		/// <value>The handle.</value>
//		public IntPtr Handle
//		{
//			get { return handle; }
//			set
//			{
//				if (handle != IntPtr.Zero)
//					Dispose();
//				handle = value;
//			}
//		}

//		static TFDisposable()
//		{
//			TFCore.Init();
//		}

//		/// <summary>
//		/// Initializes a new instance of the <see cref="T:TensorFlow.TFDisposable"/> class.
//		/// </summary>
//		public TFDisposable()
//		{ }

//		/// <summary>
//		/// Initializes a new instance of the <see cref="T:TensorFlow.TFDisposable"/> class
//		/// from the handle that it will wrap.   
//		/// </summary>
//		public TFDisposable(IntPtr handle)
//		{
//			this.handle = handle;
//		}

//		/// <summary>
//		/// Releases all resource used by the <see cref="T:TensorFlow.TFDisposable"/> object.
//		/// </summary>
//		/// <remarks>Call Dispose when you are finished using the <see cref="T:TensorFlow.TFDisposable"/>. The
//		/// Dispose method leaves the <see cref="T:TensorFlow.TFDisposable"/> in an unusable state. After
//		/// calling Dispose, you must release all references to the <see cref="T:TensorFlow.TFDisposable"/> so
//		/// the garbage collector can reclaim the memory that the <see cref="T:TensorFlow.TFDisposable"/> was occupying.</remarks>
//		public void Dispose()
//		{
//			Dispose(true);
//			GC.SuppressFinalize(this);
//		}

//		~TFDisposable()
//		{
//			Dispose(false);
//		}

//		// Must be implemented in subclasses to dispose the unmanaged object, it does
//		// not need to take care of zeroing out the handle, that is done by the Dispose
//		// method inherited from TFDisposable
//		internal abstract void NativeDispose(IntPtr handle);

//		/// <summary>
//		/// Dispose the specified object
//		/// </summary>
//		/// <param name="disposing">If set to <c>true</c> it means that this method was called from Dispose, otherwise from the finalizer.</param>
//		public virtual void Dispose(bool disposing)
//		{
//			if (disposing)
//			{
//				if (Handle != IntPtr.Zero)
//					NativeDispose(Handle);
//				handle = IntPtr.Zero;
//			}
//		}

//		internal static void ObjectDisposedException()
//		{
//			throw new ObjectDisposedException("The object was disposed");
//		}
//	}

//	partial class TFImportGraphDefOptions : TFDisposable
//    {
//		[DllImport(NativeBinding.TensorFlowLibrary)]
//        static extern unsafe void TF_ImportGraphDefOptionsSetDefaultDevice(TF_ImportGraphDefOptions opts, string prefix);

//        public void SetDefaultDevice(string device)
//        {
//            if (Handle == IntPtr.Zero)
//                ObjectDisposedException();
//            TF_ImportGraphDefOptionsSetDefaultDevice(Handle, device);
//        }

//		// extern TF_ImportGraphDefOptions * TF_NewImportGraphDefOptions ();
//		[DllImport(NativeBinding.TensorFlowLibrary)]
//		static extern unsafe TF_ImportGraphDefOptions TF_NewImportGraphDefOptions();

//		public TFImportGraphDefOptions() : base(TF_NewImportGraphDefOptions())
//		{
//		}

//		// extern void TF_DeleteImportGraphDefOptions (TF_ImportGraphDefOptions *opts);
//		[DllImport(NativeBinding.TensorFlowLibrary)]
//		static extern unsafe void TF_DeleteImportGraphDefOptions(TF_ImportGraphDefOptions opts);

//		internal override void NativeDispose(IntPtr handle)
//		{
//			TF_DeleteImportGraphDefOptions(handle);
//		}
//	}
//}
