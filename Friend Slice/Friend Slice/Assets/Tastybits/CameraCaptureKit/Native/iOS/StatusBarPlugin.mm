extern UIViewController *UnityGetGLViewController(); // Root view controller of Unity screen.
extern void UnitySendMessage( const char * className, const char * methodName, const char * param );


extern "C" void _SystemBarHelper_setStatusBarHidden( bool value ) {
	[[UIApplication sharedApplication] setStatusBarHidden:(value ? YES : NO)];
}




