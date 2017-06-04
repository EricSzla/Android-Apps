#import <AVFoundation/AVFoundation.h>
#import <UIKit/UIKit.h>
extern UIViewController *UnityGetGLViewController(); // Root view controller of Unity screen.
extern void UnitySendMessage( const char * className, const char * methodName, const char * param );
#include <memory>


@interface CameraPermissionsAlertViewDelegate : NSObject<UIAlertViewDelegate>
typedef void (^AlertViewCompletionBlock)(NSInteger buttonIndex);
@property (strong,nonatomic) AlertViewCompletionBlock callback;
+ (void)showAlertView:(UIAlertView *)alertView withCallback:(AlertViewCompletionBlock)callback;
@end


@implementation CameraPermissionsAlertViewDelegate
@synthesize callback;
- (void)alertView:(UIAlertView *)alertView clickedButtonAtIndex:(NSInteger)buttonIndex {
    callback(buttonIndex);
}
+ (void)showAlertView:(UIAlertView *)alertView
         withCallback:(AlertViewCompletionBlock)callback {
    __block CameraPermissionsAlertViewDelegate *delegate = [[CameraPermissionsAlertViewDelegate alloc] init];
    alertView.delegate = delegate;
    delegate.callback = ^(NSInteger buttonIndex) {
        callback(buttonIndex);
        alertView.delegate = nil;
        delegate = nil;
    };
    [alertView show];
}
@end



char* gCameraPermissionsCallbackObject = 0;



void InvokeCameraPermissionCallback( BOOL granted ) {
    char* tmp = gCameraPermissionsCallbackObject;
    gCameraPermissionsCallbackObject = 0;
    UnitySendMessage( tmp, "CallDelegateFromNative", granted == YES ? "{ \"granted\":true }" : "{ \"granted\":false }" );
    free( tmp );
}


void PrepareCameraCallback( const char* callbackObject ) {
    gCameraPermissionsCallbackObject = (char*)malloc( strlen(callbackObject)+2 );
    strcpy( gCameraPermissionsCallbackObject, callbackObject );
}


extern "C" int _CameraPermissions_AquireAuth( const char* callbackObject ) {
    if( gCameraPermissionsCallbackObject!=0 ) {
        NSLog(@"Error - there is already a request to aquire the permissions");
        return -1;
    }
    AVAuthorizationStatus authStatus = [AVCaptureDevice authorizationStatusForMediaType:AVMediaTypeVideo];
    if( authStatus == AVAuthorizationStatusAuthorized) {
        return 1; // authhrorurzed.
    } else if(authStatus == AVAuthorizationStatusNotDetermined) {
        NSLog( @"CameraPermissions: iOSNative: Camera access not determined. Ask for permission." );
        PrepareCameraCallback( callbackObject );
        [AVCaptureDevice requestAccessForMediaType:AVMediaTypeVideo completionHandler:
            ^( BOOL granted ) {
                InvokeCameraPermissionCallback(granted);
            }
        ];
        return 2; // not determined.
    } else if (authStatus == AVAuthorizationStatusRestricted) {
        NSLog( @"CameraPermissions: iOSNative: AVAuthorizationStatusRestricted - showing messagebox with warning." );
        PrepareCameraCallback( callbackObject );
        UIAlertView* alert = [[UIAlertView alloc] initWithTitle:@"No camera access"
                                    message:@"You can enable Camera access in the Privacy Settings for the app."
                                   delegate:nil
                          cancelButtonTitle:@"OK"
                          otherButtonTitles:nil];
        [CameraPermissionsAlertViewDelegate showAlertView:alert withCallback:^(NSInteger buttonIndex) {
            InvokeCameraPermissionCallback(NO);
        }];
        return 3; // Restricted? whatever that means...
    } else if (authStatus == AVAuthorizationStatusDenied ) {
        NSLog( @"CameraPermissions: iOSNative: AVAuthorizationStatusDenied - showing messagebox with warning." );
        PrepareCameraCallback( callbackObject );
        UIAlertView* alert = [[UIAlertView alloc] initWithTitle:@"No camera access"
                                                        message:@"You can enable Camera access in the Privacy Settings for the app."
                                                       delegate:nil
                                              cancelButtonTitle:@"OK"
                                              otherButtonTitles:nil];
        [CameraPermissionsAlertViewDelegate showAlertView:alert withCallback:^(NSInteger buttonIndex) {
            InvokeCameraPermissionCallback(NO);
        }];
        return 4;
    } else {
        
    }
    
    NSLog( @"CameraPermissions: iOSNative: Unknown permission status" );
    
    return 0; // undefined
}


