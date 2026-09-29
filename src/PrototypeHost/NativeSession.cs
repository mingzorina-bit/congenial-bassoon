using System.Runtime.InteropServices;
namespace BilingualInput;
internal sealed class NativeSession : IDisposable {
 private IntPtr handle;
 private const string Dll = "BilingualNative.dll";
 [DllImport(Dll, CallingConvention=CallingConvention.Cdecl)] private static extern IntPtr bi_create([MarshalAs(UnmanagedType.LPUTF8Str)] string shared,[MarshalAs(UnmanagedType.LPUTF8Str)] string user);
 [DllImport(Dll, CallingConvention=CallingConvention.Cdecl)] private static extern void bi_destroy(IntPtr h);
 [DllImport(Dll, CallingConvention=CallingConvention.Cdecl)] private static extern int bi_ready(IntPtr h);
 [DllImport(Dll, CallingConvention=CallingConvention.Cdecl)] private static extern int bi_type(IntPtr h,int ch);
 [DllImport(Dll, CallingConvention=CallingConvention.Cdecl)] private static extern int bi_key(IntPtr h,int key,int shift);
 [DllImport(Dll, CallingConvention=CallingConvention.Cdecl)] private static extern int bi_select(IntPtr h,int index);
 [DllImport(Dll, CallingConvention=CallingConvention.Cdecl)] private static extern void bi_highlight(IntPtr h,int index);
 [DllImport(Dll, CallingConvention=CallingConvention.Cdecl)] private static extern IntPtr bi_raw(IntPtr h);
 [DllImport(Dll, CallingConvention=CallingConvention.Cdecl)] private static extern int bi_count(IntPtr h);
 [DllImport(Dll, CallingConvention=CallingConvention.Cdecl)] private static extern int bi_highlighted(IntPtr h);
 [DllImport(Dll, CallingConvention=CallingConvention.Cdecl)] private static extern IntPtr bi_candidate(IntPtr h,int index);
 [DllImport(Dll, CallingConvention=CallingConvention.Cdecl)] private static extern IntPtr bi_take_commit(IntPtr h);
 [DllImport(Dll, CallingConvention=CallingConvention.Cdecl)] private static extern int bi_select_shadow(IntPtr h,int index);
 [DllImport(Dll, CallingConvention=CallingConvention.Cdecl)] private static extern int bi_shadow_count(IntPtr h);
 [DllImport(Dll, CallingConvention=CallingConvention.Cdecl)] private static extern IntPtr bi_shadow_field(IntPtr h,int index,int field);
 public record ShadowItem(string Text,string Language,string SourceId,string EntryId);
 public ShadowItem[] Shadows=>Enumerable.Range(0,bi_shadow_count(handle)).Select(i=>new ShadowItem(Copy(bi_shadow_field(handle,i,0)),Copy(bi_shadow_field(handle,i,1)),Copy(bi_shadow_field(handle,i,2)),Copy(bi_shadow_field(handle,i,3)))).ToArray();
 public bool SelectShadow(int i)=>bi_select_shadow(handle,i)!=0;
 public NativeSession() {
  var user = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"BilingualInput","rime-v002");
  handle=bi_create(Path.Combine(AppContext.BaseDirectory,"data","rime"),user);
  if(handle==IntPtr.Zero) throw new InvalidOperationException("Input session unavailable.");
 }
 private static string Copy(IntPtr ptr)=>Marshal.PtrToStringUTF8(ptr)??"";
 public bool Ready=>bi_ready(handle)!=0;
 public string Raw=>Copy(bi_raw(handle));
 public int Highlighted=>bi_highlighted(handle);
 public string[] Candidates=>Enumerable.Range(0,bi_count(handle)).Select(i=>Copy(bi_candidate(handle,i))).ToArray();
 public void Type(char ch)=>bi_type(handle,ch);
 public bool Key(int key,bool shift=false)=>bi_key(handle,key,shift?1:0)!=0;
 public bool Select(int i)=>bi_select(handle,i)!=0;
 public void Highlight(int i)=>bi_highlight(handle,i);
 public string TakeCommit()=>Copy(bi_take_commit(handle));
 public void Dispose(){if(handle!=IntPtr.Zero){bi_destroy(handle);handle=IntPtr.Zero;}}
}

