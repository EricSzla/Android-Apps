#import <AVFoundation/AVFoundation.h>
#import <UIKit/UIKit.h>
extern UIViewController *UnityGetGLViewController(); // Root view controller of Unity screen.
extern void UnitySendMessage( const char * className, const char * methodName, const char * param );


extern "C" const char* _GetDocumentDirectory() {
    NSArray* paths = NSSearchPathForDirectoriesInDomains( NSDocumentDirectory, NSUserDomainMask, YES );
    NSArray* paths2 = NSSearchPathForDirectoriesInDomains( NSDocumentDirectory, NSAllDomainsMask, YES );
    
    NSString* basePath = @"";
    if( [paths count] > 0 ) {
        basePath = [NSString stringWithFormat:@"%@", [paths objectAtIndex:0] ];
    }
    
    const char* str = [basePath UTF8String];
    const char* ret = strcpy((char*)malloc(strlen(str)+1), str);
    return ret;
}


extern "C" const char* _Documents_WriteToSavedPhotosAlbum( const char* fn_dummy, unsigned char bytes[], unsigned int nBytes )
{
    NSData *pictureData = [NSData dataWithBytes:bytes length:nBytes];
    UIImage *uiimg = [[UIImage alloc]initWithData:pictureData];
    UIImageWriteToSavedPhotosAlbum( uiimg, nil, nil, nil );
}


