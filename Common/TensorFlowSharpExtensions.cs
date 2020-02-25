using System;
using System.Runtime.InteropServices;
using TensorFlow;
using TF_ImportGraphDefOptions = System.IntPtr;

namespace gvaduha.Common
{
	class TFImportGraphDefOptionsExt : TFImportGraphDefOptions
	{
		// See NativeBinding for [DllImport(NativeBinding.TensorFlowLibrary)]
		[DllImport("libtensorflow")]
		static extern unsafe void TF_ImportGraphDefOptionsSetDefaultDevice(TF_ImportGraphDefOptions opts, string prefix);

		public void SetDefaultDevice(string device)
		{
			if (Handle == IntPtr.Zero)
				throw new ObjectDisposedException(nameof(SetDefaultDevice));
			TF_ImportGraphDefOptionsSetDefaultDevice(Handle, device);
		}
	}
}