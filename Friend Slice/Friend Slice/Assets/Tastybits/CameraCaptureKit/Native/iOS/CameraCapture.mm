#include "CameraCapture.h"
#include "AVCapture.h"
#include "CMVideoSampling.h"
#include "CVTextureCache.h"

#import <UIKit/UIKit.h>
#import <CoreVideo/CoreVideo.h>
#import <Accelerate/Accelerate.h>

#include <cmath>
#include <cstdlib>     /* strtoul */



@interface NSDictionary (BVJSONString)
-(NSString*) CamCap_dictToJsonStr:(BOOL) prettyPrint;
@end
@implementation NSDictionary (BVJSONString)
-(NSString*) CamCap_dictToJsonStr:(BOOL) prettyPrint {
    NSError *error;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:self
                                                       options:(NSJSONWritingOptions)    (prettyPrint ? NSJSONWritingPrettyPrinted : 0)
                                                         error:&error];
    
    if (! jsonData) {
        NSLog(@"CamCap_dictToJsonStr: error: %@", error.localizedDescription);
        return @"{}";
    } else {
        return [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    }
}
@end

// Helper method to create C string copy
char* MakeStringCopy (const char* string) {
    if (string == NULL)
        return NULL;
    
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}


unsigned int x2uint( const char * hexstring ) {
    unsigned int	i = 0;
    
    if ((*hexstring == '0') && (*(hexstring+1) == 'x'))
        hexstring += 2;
    while (*hexstring)
    {
        char c = toupper( *hexstring++ );
        if ((c < '0') || (c > 'F') || ((c > '9') && (c < 'A')))
            break;
        c -= '0';
        if (c > 9)
            c -= 7;
        i = (i << 4) + c;
    }
    return i;
}


unsigned long x2ulong( const char * hexstring ) {
    unsigned long	i = 0;
    
    if ((*hexstring == '0') && (*(hexstring+1) == 'x'))
        hexstring += 2;
    while (*hexstring)
    {
        char c = toupper(*hexstring++);
        if ((c < '0') || (c > 'F') || ((c > '9') && (c < 'A')))
            break;
        c -= '0';
        if (c > 9)
            c -= 7;
        i = (i << 4) + c;
    }
    return i;
}


void* str2ptr( const char* str ) {
#ifdef __LP64__
    unsigned long tmp = x2ulong( str );
    return (void*)tmp;
#else
    uint tmp = x2uint( str );
    return (void*)tmp;
#endif
}



@interface NSDictionary (BVJSONString)
-(NSString*) CamCap_dictToJsonStr:(BOOL) prettyPrint;
@end

CameraCaptureController* CameraCaptureController_inst;
void* CameraCaptureController_lastCreatedRef=0;
BOOL NextCaptureSessionIsStillImage = NO; // use this to flag that we are capturing a still image.
BOOL NextCaptureSessionQR=NO;

@implementation CameraCaptureController
{
    AVCaptureDevice*			_captureDevice;
    AVCaptureSession*			_captureSession;
    AVCaptureDeviceInput*		_captureInput;
    AVCaptureVideoDataOutput*	_captureOutput;
    AVCaptureStillImageOutput*  _captureStillImageOutput;
    AVCaptureMetadataOutput*    _captureQROutput;
    
    @public CMVideoSampling     _cmVideoSampling;
    @public void*               _userData;
    @public size_t              _width, _height;
    
    @public BOOL                _handbreak;
    @public BOOL                _handbreak2;
    
    @public BOOL                isStillImage;
    @public BOOL                doCaptureQR;
    
    NSString*                   _captureStillImageCB;
    NSString*                   _firstFrameCallback;
    @public NSInteger           exposureCompensation;
    @public NSInteger           requestedISO;
}



- (bool)initCapture:(AVCaptureDevice*)device width:(int)w height:(int)h fps:(float)fps {
    if( UnityGetAVCapturePermission(avVideoCapture) == avCapturePermissionDenied )
        return false;
    
    self.captureDevice = device;
    
    self.captureInput	= [AVCaptureDeviceInput deviceInputWithDevice:device error:nil];
    self.captureOutput	= [[AVCaptureVideoDataOutput alloc] init];
    
    self->isStillImage = NextCaptureSessionIsStillImage;
    NextCaptureSessionIsStillImage = NO;
    
    self->doCaptureQR = NextCaptureSessionQR;
    NextCaptureSessionQR = NO;
    
    if( self->isStillImage == YES ) {
        self.captureStillImageOutput = [[AVCaptureStillImageOutput alloc] init];
    } else {
        self.captureStillImageOutput = nil;
    }
    
    if( self.captureOutput == nil || (self.captureStillImageOutput==nil && self->isStillImage) || self.captureInput == nil ) {
        NSLog(@"Error initializing still image capturing session");
        return false;
    }
    
    self->_handbreak = false;
    self->_handbreak2 = false;
    
    
    self.captureOutput.alwaysDiscardsLateVideoFrames = YES;
    
    if([device lockForConfiguration:nil]) {
        AVFrameRateRange* range = [self pickFrameRateRange:fps];
        if( range ) {
            if([device respondsToSelector:@selector(activeVideoMinFrameDuration)])
                device.activeVideoMinFrameDuration = range.minFrameDuration;
            if([device respondsToSelector:@selector(activeVideoMaxFrameDuration)])
                device.activeVideoMaxFrameDuration = range.maxFrameDuration;
        } else {
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wdeprecated-declarations"
            self.captureOutput.minFrameDuration = CMTimeMake(1, fps);
#pragma clang diagnostic pop
        }
        if( [device isFlashModeSupported:AVCaptureFlashModeOff] ) {
            [device setFlashMode:(AVCaptureFlashMode)AVCaptureFlashModeOff];
        }
        
        [device unlockForConfiguration];
    }
    
    self->exposureCompensation = 0;
    self->requestedISO = -999;
    
    // queue on main thread to simplify gles life
    [self.captureOutput setSampleBufferDelegate:self queue:dispatch_get_main_queue()];
    
    NSDictionary* options = @{ (NSString*)kCVPixelBufferPixelFormatTypeKey : @(kCVPixelFormatType_32BGRA) };
    [self.captureOutput setVideoSettings:options];
    
    if( self->isStillImage == YES ) {
        [self.captureStillImageOutput setOutputSettings:options];
    }
    
    self.captureSession = [[AVCaptureSession alloc] init];
    [self.captureSession addInput:self.captureInput];
    [self.captureSession addOutput:self.captureOutput];
    
    if( self->isStillImage == YES ) {
        [self.captureSession addOutput:self.captureStillImageOutput];
        self.captureSession.sessionPreset = AVCaptureSessionPresetPhoto;
        //self.captureSession.sessionPreset = AVCaptureSessionPresetHigh;
    } else {
        self.captureSession.sessionPreset = [self pickPresetFromWidth:w height:h];
    }

    if( self->doCaptureQR ) {
        self.captureQROutput = [[AVCaptureMetadataOutput alloc] init];
        // Have to add the output before setting metadata types
        [self.captureSession addOutput:self.captureQROutput];
        // What different things can we register to recognise?
        //NSLog(@"%@", [self.captureQROutput availableMetadataObjectTypes]);
        [self.captureQROutput setMetadataObjectTypes:@[AVMetadataObjectTypeQRCode]];
        [self.captureQROutput setMetadataObjectsDelegate:self queue:dispatch_get_main_queue()];
    }

    
    CMVideoSampling_Initialize(&self->_cmVideoSampling);
    
    _width = _height = 0;
    self.firstFrameCallback = nil;
    
    return true;
}


//#pragma mark - AVCaptureMetadataOutputObjectsDelegate
- (void)captureOutput:(AVCaptureOutput *)captureOutput didOutputMetadataObjects:(NSArray *)metadataObjects fromConnection:(AVCaptureConnection *)connection
{
    for (AVMetadataObject *metadata in metadataObjects) {
        if ([metadata.type isEqualToString:AVMetadataObjectTypeQRCode]) {
            AVMetadataMachineReadableCodeObject *transformed = (AVMetadataMachineReadableCodeObject *)metadata;
            // Update the view with the decoded text
            //_decodedMessage.text = [transformed stringValue];
        }
    }
}


//#pragma mark - AVCaptureMetadataOutputObjectsDelegate

- (void)captureOutput:(AVCaptureOutput*)captureOutput didOutputSampleBuffer:(CMSampleBufferRef)sampleBuffer fromConnection:(AVCaptureConnection*)connection
{
    /*CVImageBufferRef imageBuffer = CMSampleBufferGetImageBuffer(sampleBuffer);
     if(imageBuffer != NULL) {
     CGSize imageSize = CVImageBufferGetDisplaySize(imageBuffer);
     NSLog(@"%f , %f", imageSize.width, imageSize.height );
     }*/
    if( self->_handbreak || self->_handbreak2 ) {
        NSLog(@"ignored decoded preview frame becaurse of handbreak. 1 or 2");
        return;
    }
    intptr_t tex = (intptr_t)CMVideoSampling_SampleBuffer(&self->_cmVideoSampling, sampleBuffer, &self->_width, &self->_height);
    
    UnityDidCaptureVideoFrame(tex, self->_userData);
    
    if( self.firstFrameCallback!=nil ) {
        [self fireFirstFrame];
    }
}


- (void)start
{
    self->_handbreak = self->_handbreak2 = false;
    [self.captureSession startRunning];
}


- (void)pause
{
    [self.captureSession stopRunning];
}


- (void)stop
{
    if( self.captureSession==nil ) {
        return;
    }
    
    
    [self.captureSession stopRunning];
    [self.captureSession removeInput: self.captureInput];
    [self.captureSession removeOutput: self.captureStillImageOutput];
    self.captureDevice = nil;
    self.captureInput = nil;
    self.captureOutput = nil;
    self.captureSession = nil;
    self.captureStillImageOutput = nil;
    self.captureStillImageCB = nil;
    self->isStillImage = NO;
    self.firstFrameCallback == nil;
    self->exposureCompensation = 0;
    self->requestedISO = -999;
    
    CMVideoSampling_Uninitialize(&self->_cmVideoSampling);
}



-(void) setCaptureStillCallback:(NSString *)callback {
    if( self->isStillImage == NO ) {
        NSLog(@"Not capturing still image");
        return;
    }
    self.captureStillImageCB = callback;
}


-(void) fireFirstFrame {
    if( self.firstFrameCallback == nil ) {
        return;
    }
    NSDictionary* dictRet = [[NSDictionary alloc] initWithObjectsAndKeys: @"true", @"succeeded", nil];
    UnitySendMessage( [self.firstFrameCallback UTF8String], "CallDelegateFromNative", [[dictRet CamCap_dictToJsonStr:TRUE] UTF8String] );
    self.firstFrameCallback = nil;
}


-(void) fireCaptureStillCallback {
    if( self->isStillImage == NO ) {
        NSLog(@"Not capturing still image");
        return;
    }
    if( self.captureStillImageCB == nil ) {
        NSLog(@"No callback set");
        return;
    }
    NSDictionary * dictRet = [[NSDictionary alloc] initWithObjectsAndKeys: @"true", @"succeeded", nil];
    UnitySendMessage( [self.captureStillImageCB UTF8String], "CallDelegateFromNative", [[dictRet CamCap_dictToJsonStr:TRUE] UTF8String] );
    self.captureStillImageCB = nil;
}


- (void) captureStillImage {
    if( self->isStillImage == NO ) {
        NSLog(@"Not capturing still image");
        return;
    }
    if( ![self.captureSession isRunning] || self->_handbreak ) {
        NSLog(@"CaptureSession is not running");
        return;
    }
    
    id videoConnection = [self.captureStillImageOutput connectionWithMediaType:AVMediaTypeVideo];
    
    // Pause the preview output.
    self->_handbreak2 = true;
    
    //[self.captureSession stopRunning];
    [self.captureStillImageOutput captureStillImageAsynchronouslyFromConnection:videoConnection completionHandler: ^(CMSampleBufferRef sampleBuffer, NSError *error)
     {
         intptr_t tex = (intptr_t)CMVideoSampling_SampleBuffer( &self->_cmVideoSampling, sampleBuffer, &self->_width, &self->_height);
         UnityDidCaptureVideoFrame(tex, self->_userData);
         
         self->_handbreak = true;
         [self.captureSession stopRunning];
         
         [self fireCaptureStillCallback];
     }];
}




- (NSString*)pickPresetFromWidth:(int)w height:(int)h
{
    static NSString* preset[] =
    {
        AVCaptureSessionPreset352x288,
        AVCaptureSessionPreset640x480,
        AVCaptureSessionPreset1280x720,
        AVCaptureSessionPreset1920x1080,
    };
    static int presetW[] = { 352, 640, 1280, 1920 };
    
    //[AVCamViewController setFlashMode:AVCaptureFlashModeAuto forDevice:[[self videoDeviceInput] device]];
    
    
#define countof(arr) sizeof(arr)/sizeof(arr[0])
    
    static_assert(countof(presetW) == countof(preset), "preset and preset width arrrays have different elem count");
    
    int ret = -1, curW = -10000;
    for(int i = 0, n = countof(presetW) ; i < n ; ++i)
    {
        if( ::abs(w - presetW[i]) < ::abs(w - curW) && [self.captureSession canSetSessionPreset:preset[i]] )
        {
            ret = i;
            curW = presetW[i];
        }
    }
    
    NSAssert(ret != -1, @"Cannot pick capture preset");
    return ret != -1 ? preset[ret] : AVCaptureSessionPresetHigh;
    
#undef countof
}


- (AVFrameRateRange*)pickFrameRateRange:(float)fps
{
    AVFrameRateRange* ret = nil;
    
    if([self.captureDevice respondsToSelector:@selector(activeFormat)])
    {
        float minDiff = INFINITY;
        
        // In some corner cases (seeing this on iPod iOS 6.1.5) activeFormat is null.
        if (!self.captureDevice.activeFormat)
            return nil;
        
        for(AVFrameRateRange* rate in self.captureDevice.activeFormat.videoSupportedFrameRateRanges) {
            float bestMatch = rate.minFrameRate;
            if (fps > rate.maxFrameRate)		bestMatch = rate.maxFrameRate;
            else if (fps > rate.minFrameRate)	bestMatch = fps;
            
            float diff = ::fabs(fps - bestMatch);
            if(diff < minDiff)
            {
                minDiff = diff;
                ret = rate;
            }
        }
        
        NSAssert(ret != nil, @"Cannot pick frame rate range");
        if( ret == nil ) {
            ret = self.captureDevice.activeFormat.videoSupportedFrameRateRanges[0];
        }
    }
    return ret;
}

@synthesize captureDevice	= _captureDevice;
@synthesize captureSession	= _captureSession;
@synthesize captureOutput	= _captureOutput;
@synthesize captureStillImageOutput = _captureStillImageOutput;
@synthesize captureInput	= _captureInput;
@synthesize captureStillImageCB = _captureStillImageCB;
@synthesize firstFrameCallback = _firstFrameCallback;
@synthesize captureQROutput = _captureQROutput;
@end


// added by KRM:
extern "C" void UnityCaptureStillImage( const char* spcapture, char* callback ) {
    CameraCaptureController* i1 = CameraCaptureController_inst;
    void* pcapture = str2ptr(spcapture);
    //CameraCaptureController* i1 = CameraCaptureController_inst;
    CameraCaptureController* controller = (__bridge CameraCaptureController*)pcapture;
    [controller setCaptureStillCallback:[NSString stringWithFormat:@"%s",callback]];
    [controller captureStillImage];
}


extern "C" void UnitySetFirstFrameCallback( const char* spcapture, const char* callback ) {
    CameraCaptureController* i1 = CameraCaptureController_inst;
    void* pcapture = str2ptr(spcapture);
    CameraCaptureController* controller = (__bridge CameraCaptureController*)pcapture;
    controller.firstFrameCallback = [NSString stringWithFormat:@"%s",callback];
}


extern "C" void	UnityEnumVideoCaptureDevices( void* udata, void(*callback)(void* udata, const char* name, int frontFacing)) {
    for (AVCaptureDevice* device in [AVCaptureDevice devicesWithMediaType:AVMediaTypeVideo]) {
        int frontFacing = device.position == AVCaptureDevicePositionFront ? 1 : 0;
        callback(udata, [device.localizedName UTF8String], frontFacing);
    }
}


extern "C" void* UnityInitCameraCapture( int deviceIndex, int w, int h, int fps, void* udata ) {
    AVCaptureDevice* device = [AVCaptureDevice devicesWithMediaType:AVMediaTypeVideo][deviceIndex];
    CameraCaptureController* controller = [CameraCaptureController alloc];
    CameraCaptureController_inst = controller;
    if( [controller initCapture:device width:w height:h fps:(float)fps] ) {
        controller->_userData = udata;
        void* ptr = CameraCaptureController_lastCreatedRef = (__bridge_retained void*)controller;
        return ptr;
    }
    controller = nil;
    return 0;
}


extern "C" const char* UnityGetLastCameraCapturePointer() {
    if( CameraCaptureController_lastCreatedRef == 0 ) {
        NSLog(@"Error returning last created pointer is null");
    }
    
    const void * address = static_cast<const void*>(CameraCaptureController_lastCreatedRef);
    char* str = new char[16+1]; str[0] = 0;
    sprintf( str,"%p",address );
    
#ifdef __LP64__
    unsigned long tmp = x2ulong( str );
#else
    uint tmp = x2uint( str );
#endif
    
    char* str2 = new char[16+1]; str2[0] = 0;
    sprintf( str2,"%p", (void*)tmp );
    NSLog(@"Info: WebCamTexture pointer converted %s - and back again %s", str, str2  );
    
    return str;
}



extern "C" int UnityCaptureGetTorchMode( const char* spcapture) {
    void* pcapture = str2ptr(spcapture);
    CameraCaptureController* controller = (__bridge CameraCaptureController*)pcapture;
    int ret = (int)controller.captureDevice.torchMode;
    return ret;
}



extern "C" void UnityStartCameraCapture(void* capture) {
    [(__bridge CameraCaptureController*)capture start];
}


extern "C" void UnityPauseCameraCapture(void* capture) {
    [(__bridge CameraCaptureController*)capture pause];
}


// Note: this is simple way of adding breakpoints where we can
// inspect the memory increase inside between c# code.
// ( it's not used for anything other than developing this plugin )
extern "C" void UnityBreakPoint( char* msg ) {
    NSLog(@"test break %s", msg);
    NSLog(@" " );
}


extern "C" void UnityStopCameraCapture(void* capture) {
    CameraCaptureController* controller = (__bridge_transfer CameraCaptureController*)capture;
    if( CameraCaptureController_lastCreatedRef == capture ) {
        CameraCaptureController_lastCreatedRef = 0;
    }
    [controller stop];
    controller = nil;
}


extern "C" bool UnityCaptureHasFlash( const char* spcapture ) {
    void *pcapture = str2ptr(spcapture);
    CameraCaptureController* controller = (__bridge CameraCaptureController*)(void*)pcapture;
    return controller.captureDevice.hasFlash == YES ? true:false;
}


extern "C" bool UnityFlashModeSupported( const char* spcapture, int mode ) {
    void *pcapture = str2ptr(spcapture);
    CameraCaptureController* controller = (__bridge CameraCaptureController*)(void*)pcapture;
    BOOL ok = [controller.captureDevice isFlashModeSupported:(AVCaptureFlashMode)mode];
    return ok == YES ? true:false;
}


extern "C" void UnitySetFlashMode( const char* spcapture, int value ) {
    //NSLog(@"ptr = %s value = %i", spcapture, value );
    void *pcapture = str2ptr(spcapture);
    CameraCaptureController* controller = (__bridge CameraCaptureController*)(void*)pcapture;
    if( controller == nil ) {
        NSLog(@"error the ptr was not resolved");
    }
    if( value>2 || value < 0 ) {
        NSLog(@"Error: Invalid flash value given.");
    }
    NSInteger* availModes = new NSInteger[ 3 ]{
        AVCaptureFlashModeOff,
        AVCaptureFlashModeOn,
        AVCaptureFlashModeAuto
    };
    NSInteger _mode = availModes[ value % 3 ];
    if([controller.captureDevice lockForConfiguration:nil]) {
        NSLog(@"Setting flash mode to : %ld", (long)_mode );
        if( [controller.captureDevice isFlashModeSupported:(AVCaptureFlashMode)_mode] ) {
            [controller.captureDevice setFlashMode:(AVCaptureFlashMode)_mode];
        } else {
            NSLog(@"Error flash mode : %ld is not supported", (long)_mode );
        }
        [controller.captureDevice unlockForConfiguration];
    } else {
        NSLog(@"Failed to lock captureDevice");
    }
}


extern "C" bool UnityCaptureGetHasTorch( const char* spcapture ) {
    void *pcapture = str2ptr(spcapture);
    CameraCaptureController* controller = (__bridge CameraCaptureController*)(void*)pcapture;
    return controller.captureDevice.hasTorch == YES ? true:false;
}


extern "C" bool UnityTorchModeSupported( const char* spcapture, int mode ) {
    void *pcapture = str2ptr(spcapture);
    CameraCaptureController* controller = (__bridge CameraCaptureController*)(void*)pcapture;
    BOOL ok = [controller.captureDevice isFlashModeSupported:(AVCaptureFlashMode)mode];
    return ok == YES ? true:false;
}


extern "C" void UnityCaptureSetTorchMode( const char* spcapture, int value, char* callback ) {
    void *pcapture = str2ptr(spcapture);
    AVCaptureTorchMode _mode = AVCaptureTorchModeOff;
    if( value == 0 ){
        _mode = AVCaptureTorchModeOff;
    } else if( value == 1 ) {
        _mode = AVCaptureTorchModeOn;
    } else if( value == 2 ) {
        _mode = AVCaptureTorchModeAuto;
    } else {
        NSLog(@"invalid torch mode : %i", value);
    }
    
    CameraCaptureController* controller = (__bridge CameraCaptureController*)(void*)pcapture;
    if( [controller.captureDevice hasTorch] == NO ) {
        NSLog(@"device has no torch");
        return;
    }
    if( [controller.captureDevice  lockForConfiguration:nil] ) {
        if( [controller.captureDevice isTorchModeSupported: _mode] == YES ) {
            [controller.captureDevice  setTorchMode:(AVCaptureTorchMode)_mode];
        } else {
            NSLog( @"Error torch mode is not supported");
        }
        [controller.captureDevice  unlockForConfiguration];
    }
}


extern "C" bool UnityIsFocusModeSupported( const char* spcapture, int _mode ) {
    void *pcapture = str2ptr(spcapture);
    AVCaptureFocusMode mode = AVCaptureFocusModeAutoFocus;
    if( _mode == 0 ) {
        mode = AVCaptureFocusModeLocked;
    } else if( _mode == 1 ) {
        mode = AVCaptureFocusModeAutoFocus;
    } else if( _mode == 2 ) {
        mode = AVCaptureFocusModeContinuousAutoFocus;
    }
    CameraCaptureController* controller = (__bridge CameraCaptureController*)(void*)pcapture;
    BOOL yes = [controller.captureDevice isFocusModeSupported:(AVCaptureFocusMode)mode];
    return yes==YES?true:false;
}


extern "C" void UnitySetFocusMode( const char* spcapture, int _mode ) {
    void *pcapture = str2ptr(spcapture);
    AVCaptureFocusMode mode = AVCaptureFocusModeAutoFocus;
    if( _mode == 0 ) {
        mode = AVCaptureFocusModeLocked;
    } else if( _mode == 1 ) {
        mode = AVCaptureFocusModeAutoFocus;
    } else if( _mode == 2 ) {
        mode = AVCaptureFocusModeContinuousAutoFocus;
    } else {
        NSLog(@"Unknown focus mode %i ", _mode );
    }
    CameraCaptureController* controller = (__bridge CameraCaptureController*)(void*)pcapture;
    if([controller.captureDevice lockForConfiguration:nil]) {
        BOOL yes = [controller.captureDevice isFocusModeSupported:(AVCaptureFocusMode)mode];
        if( yes == YES ) {
            controller.captureDevice.focusMode = (AVCaptureFocusMode)mode;
        } else {
            NSLog(@"Focus mode not supported");
        }
        [controller.captureDevice unlockForConfiguration];
    }
}


extern "C" int UnityGetFocusMode( const char* spcapture ) {
    void *pcapture = str2ptr(spcapture);
    CameraCaptureController* controller = (__bridge CameraCaptureController*)(void*)pcapture;
    return (int)controller.captureDevice.focusMode;
}


extern "C" int UnityGetFlashMode( const char* spcapture ) {
    void *pcapture = str2ptr(spcapture);
    CameraCaptureController* controller = (__bridge CameraCaptureController*)(void*)pcapture;
    int flashMode = (int)controller.captureDevice.flashMode;
    //NSLog(@"flashMode = %i", flashMode );
    return flashMode;
}


extern "C" bool UnityCanAdjustExposureMode( const char* spcapture ) {
    void *pcapture = str2ptr(spcapture);
    CameraCaptureController* controller = (__bridge CameraCaptureController*)(void*)pcapture;
    BOOL canDoLockedMode = [controller.captureDevice isExposureModeSupported:AVCaptureExposureModeLocked];
    return canDoLockedMode == YES;
}



extern "C" void UnitySetCurrentISO( const char* spcapture, const char* str_value ); // fwddecl.
extern "C" void UnitySetExposureCompensation( const char* spcapture, int value ) {
    if( spcapture == nil ) {
        NSLog(@"Error referance to camera is null");
    }
    void *pcapture = str2ptr(spcapture);
    CameraCaptureController* controller = (__bridge CameraCaptureController*)(void*)pcapture;
    if( value > 2 ) value = 2;
    if( value < -2 ) value = -2;
    controller->exposureCompensation = value;
    
    if( controller->requestedISO == -999 ) {
        // We havent implemneting auto iso and manual.
    } else {
        NSString* str = [NSString stringWithFormat:@"%i", (int)controller->requestedISO];
        UnitySetCurrentISO( spcapture, [str UTF8String] );
    }
}


// Read more on : this is a pretty standard value : http://www.exposureguide.com/exposure.htm
extern "C" int UnityGetMinExposureCompensationValue( const char* spcapture ) {
    return -2;
}


// Read more on : this is a pretty standard value : http://www.exposureguide.com/exposure.htm
extern "C" int UnityGetMaxExposureCompensationValue( const char* spcapture ) {
    return +2;
}



// Get current iso returns the iso of the camera, however in order to translate
// the iso into human readable values we look some standard iso values up in a table
// and return one of them.
extern "C" float UnityGetCurrentISO( const char* spcapture ) {
    if( spcapture == nil ) {
        NSLog(@"Error referance to camera is null");
    }
    void *pcapture = str2ptr(spcapture);
    CameraCaptureController* controller = (__bridge CameraCaptureController*)(void*)pcapture;
 
    float iso = controller.captureDevice.ISO;
    //float minISO = controller.captureDevice.activeFormat.minISO;
    //float maxISO = controller.captureDevice.activeFormat.maxISO;
    //NSLog(@"minISO=%f maxISO=%f", minISO, maxISO );
    
    CMTime expDur = controller.captureDevice.exposureDuration;
    NSLog(@"current expDur  %f", CMTimeGetSeconds(expDur) );
    
    
    // in the beginning its not custom its auto... we will return auto like this...
    if( controller.captureDevice.exposureMode != AVCaptureExposureModeCustom ) {
        return -999; //-999 means auto...
    }
    
    
    #define MAX_NUM 8
    int* tmp = new int[MAX_NUM]{
        30,
        64,
        100,
        200,
        640,
        800,
        1600,
        1800,
    };

    int ret = (int)iso;
    for( int i = 0; i<MAX_NUM; i++ ) {
        int v1 = tmp[i];
        if( i == MAX_NUM - 1 ) {
            if( (int)iso > v1 ) {
                ret = v1;
                return ret;
            }
        }
        int v2 = tmp[i+1];
        if( v1 >= (int)iso && v2 <= (int)iso ) {
            ret = v1;
            return ret;
        }
    }
    
    return ret;
}


// Sets the current iso value, the value set might get replace with a valid value from the
// iso lookup table or iOS/theCamera migth change the value to something not exately what
// you have given.
extern "C" void UnitySetCurrentISO( const char* spcapture, const char* str_value ) {
    //NSLog(@"debug this");
    if( spcapture == nil ) {
        NSLog(@"Error referance to camera is null");
    }
    void *pcapture = str2ptr(spcapture);
    CameraCaptureController* controller = (__bridge CameraCaptureController*)(void*)pcapture;

    
    NSString* strVal = [NSString stringWithFormat:@"%s",str_value];
    if( [strVal isEqualToString:@"auto" ] ) {
        
        NSInteger mode;
        if( [controller.captureDevice isExposureModeSupported:AVCaptureExposureModeAutoExpose ] ) {
            mode = AVCaptureExposureModeAutoExpose;
        } else if( [controller.captureDevice isExposureModeSupported:AVCaptureExposureModeContinuousAutoExposure] ) {
            mode = AVCaptureExposureModeContinuousAutoExposure;
        } else {
            NSLog(@"Error: UnitySetCurrentISO - no mode is found supposed");
            return;
        }
        
        if([controller.captureDevice lockForConfiguration:nil]) {
            if( [controller.captureDevice isExposureModeSupported:(AVCaptureExposureMode)mode] ) {
                [controller.captureDevice setExposureMode:(AVCaptureExposureMode)mode];
            } else {
                NSLog(@"Error: Cannot set exposure mode %ld", (long)mode );
            }
            [controller.captureDevice unlockForConfiguration];
        }
        
    } else {
        NSInteger intValue = [strVal integerValue];
        
        #define MAX_NUM 8
        CMTime* durIntervals = new CMTime[MAX_NUM]{
            CMTimeMake(1, 1),
            CMTimeMake(1, 1),
            CMTimeMake(4, 1),
            CMTimeMake(4, 1),
            CMTimeMake(1, 30),
            CMTimeMake(1, 30),
            CMTimeMake(1, 125),
            CMTimeMake(1, 125),
        };
        int* isoLevels = new int[MAX_NUM]{
            30,
            64,
            100,
            200,
            640,
            800,
            1600,
            1800,
        };
        
        float iso = controller.captureDevice.ISO;
        int isoLevel = (int)iso;
        int curISOLevelIndex = 0;
        for( int i = 0; i<MAX_NUM; i++ ) {
            int v1 = isoLevels[i];
            if( i == MAX_NUM - 1 ) {
                if( (int)iso > v1 ) {
                    isoLevel = v1;
                    break;
                }
            }
            int v2 = isoLevels[i+1];
            if( v1 >= (int)iso && v2 <= (int)iso ) {
                isoLevel = v1;
                break;
            }
            curISOLevelIndex++;
        }
        
        
        int expIndex = curISOLevelIndex + (int)controller->exposureCompensation;
        if( expIndex < 0 ) expIndex = 0;
        if( expIndex >= MAX_NUM ) expIndex = MAX_NUM-1;
        
        
        CMTime minExp = controller.captureDevice.activeFormat.minExposureDuration;
        CMTime maxExp = controller.captureDevice.activeFormat.maxExposureDuration;
        CMTime expDur = durIntervals[expIndex]; // 5 seconds
        if( CMTimeCompare( minExp, expDur ) != -1 ) {
            expDur = minExp;
        }
        if( CMTimeCompare( expDur, maxExp ) != -1 ) {
            expDur = maxExp;
        }
        
        
        // set the exposure to the new value...
        iso = (float)intValue;
        if([controller.captureDevice lockForConfiguration:nil]) {
            if( [controller.captureDevice isExposureModeSupported:AVCaptureExposureModeCustom] ) {
                NSLog(@"setting custom exposure mode with ISO...");
                [controller.captureDevice setExposureModeCustomWithDuration:expDur ISO:iso completionHandler:^(CMTime syncTime) {
                    NSLog(@"completionHandler");
                }];
            } else {
                NSLog(@"Error: Cannot set custom exposure mode." );
            }
            [controller.captureDevice unlockForConfiguration];
        }

    }
}


// Get supported iso values
extern "C" const char* UnityGetSupportedISOValues( const char* spcapture ) {
    if( spcapture == nil ) {
        NSLog(@"Error referance to camera is null");
    }
    void *pcapture = str2ptr(spcapture);
    CameraCaptureController* controller = (__bridge CameraCaptureController*)(void*)pcapture;
    
    CMTime minExp = controller.captureDevice.activeFormat.minExposureDuration;
    CMTime maxExp = controller.captureDevice.activeFormat.maxExposureDuration;
    NSUInteger emin = (long)CMTimeGetSeconds(minExp);
    NSUInteger emax = (long)CMTimeGetSeconds(maxExp);

    float minISO = controller.captureDevice.activeFormat.minISO;
    float maxISO = controller.captureDevice.activeFormat.maxISO;
    
    //NSLog(@"Exposure compensation min=%ld max=%ld", (long)emin, (long)emax );
    //NSLog(@"minISO=%f maxISO=%f", minISO, maxISO );

    #define MAX_NUM 8
    int* tmp = new int[MAX_NUM]{
        30,
        64,
        100,
        200,
        640,
        800,
        1600,
        1800,
    };
    NSString* strArr = @"";
    for( int i = 0; i<MAX_NUM; i++ ) {
        int val = tmp[i];
        if( val >= minISO && val <= maxISO ) {
            NSString* comma = [strArr isEqualToString:@""]?@"":@",";
            
            strArr = [NSString stringWithFormat:@"%@%@%i",strArr,
                      ([strArr isEqualToString:@""]?@"":@","),
                      val ];
        }
    }
    
    // Auto is a supported value as well in this system, like on android.
    strArr = [NSString stringWithFormat:@"%@%@%@",strArr,
              ([strArr isEqualToString:@""]?@"":@","),
              @"auto"];
    
    // TODO: release this string as well.
    return MakeStringCopy( [strArr UTF8String] );
}


// For more info on how to set manual exposure with auto-iso
// see: http://stackoverflow.com/questions/22029381/avfoundation-how-to-control-exposure
// Due to what is implemented on Android we haven't implemented support for this , it could be done
// in a future revision.
extern "C" void UnityCameraSetAutoISOWithExposureValue( const char* spcapture, float duration ) {
    /*if( spcapture == nil ) {
        NSLog(@"Error referance to camera is null");
        return 0;
    }
    void *pcapture = str2ptr(spcapture);
    CameraCaptureController* controller = (__bridge CameraCaptureController*)(void*)pcapture;*/
}


// note that this might be translated into the
extern "C" int UnityCameraGetISOMin( const char* spcapture ) {
    if( spcapture == nil ) {
        NSLog(@"Error referance to camera is null");
        return 0;
    }
    void *pcapture = str2ptr(spcapture);
    CameraCaptureController* controller = (__bridge CameraCaptureController*)(void*)pcapture;
    CGFloat minISO = controller.captureDevice.activeFormat.minISO;
    return (int)minISO;
}


// Note: that this might be translated into the iso/exposure table insteda.
extern "C" int UnityCameraGetISOMax( const char* spcapture ) {
    if( spcapture == nil ) {
        NSLog(@"Error referance to camera is null");
        return 0;
    }
    void *pcapture = str2ptr(spcapture);
    CameraCaptureController* controller = (__bridge CameraCaptureController*)(void*)pcapture;
    CGFloat minISO = controller.captureDevice.activeFormat.maxISO;
    return (int)minISO;
}


// Note: this is the "same" as UnityCameraGetMaxExposureDuration
extern "C" int UnityGetMinExposureValue( const char* spcapture ) {
    return 0;
}

// Note: this is the "same" as UnityCameraGetMaxExposureDuration
extern "C" int UnityGetMaxExposureValue( const char* spcapture ) {
    return 0;
}


// Note this is not used currently since we have implemented an exposure compensation instead.
extern "C" float UnityCameraGetMaxExposureDuration( const char* spcapture ) {
    if( spcapture == nil ) {
        NSLog(@"Error referance to camera is null");
        return 0;
    }
    void *pcapture = str2ptr(spcapture);
    CameraCaptureController* controller = (__bridge CameraCaptureController*)(void*)pcapture;
    CMTime maxExp = controller.captureDevice.activeFormat.maxExposureDuration;
    NSUInteger durSec = (long)CMTimeGetSeconds(maxExp);
    return (float)durSec;
}


// Note: this is not really valid values.
extern "C" float UnityCameraGetMinExposureDuration( const char* spcapture ) {
    if( spcapture == nil ) {
        NSLog(@"Error referance to camera is null");
        return 0;
    }
    void *pcapture = str2ptr(spcapture);
    CameraCaptureController* controller = (__bridge CameraCaptureController*)(void*)pcapture;
    CMTime maxExp = controller.captureDevice.activeFormat.minExposureDuration;
    NSUInteger durSec = (long)CMTimeGetSeconds(maxExp);
    return (float)durSec;
}


// Note: used when setting iso to auto ?
extern "C" void UnitySetAutoExposureLock( const char* spcapture, bool value ) {
    NSLog(@"debug this");
    if( spcapture == nil ) {
        NSLog(@"Error referance to camera is null");
    }
    void *pcapture = str2ptr(spcapture);
    CameraCaptureController* controller = (__bridge CameraCaptureController*)(void*)pcapture;
    
    
    NSInteger mode  = AVCaptureExposureModeLocked;
    if( value ) {
        if( [controller.captureDevice isExposureModeSupported:AVCaptureExposureModeAutoExpose ] ) {
            mode = AVCaptureExposureModeLocked;
        } else {
            NSLog(@"Error: UnitySetAutoExposureLock - no mode is found supposed");
            return;
        }
    } else {
        if( [controller.captureDevice isExposureModeSupported:AVCaptureExposureModeAutoExpose ] ) {
            mode = AVCaptureExposureModeAutoExpose;
        }
        else if( [controller.captureDevice isExposureModeSupported:AVCaptureExposureModeContinuousAutoExposure] ) {
            mode = AVCaptureExposureModeContinuousAutoExposure;
        } else {
            NSLog(@"Error: UnitySetAutoExposureLock - no mode is found supposed");
            return;
        }
    }
    
    if([controller.captureDevice lockForConfiguration:nil]) {
        if( [controller.captureDevice isExposureModeSupported:(AVCaptureExposureMode)mode] ) {
            [controller.captureDevice setExposureMode:(AVCaptureExposureMode)mode];
        } else {
            NSLog(@"Error: Cannot set exposure mode %ld", (long)mode );
        }
        [controller.captureDevice unlockForConfiguration];
    }
}


// Note: currently not used.
extern "C" void UnitySetExposureMode( const char* spcapture, int value ) {
    if( value < 0 || value >= 3 ) {
        NSLog(@"Error given exposure mode is invalid number %i", value );
    }
    if( spcapture == nil ) {
        NSLog(@"Error referance to camera is null");
    }
    void *pcapture = str2ptr(spcapture);
    CameraCaptureController* controller = (__bridge CameraCaptureController*)(void*)pcapture;
    
    NSInteger* availValues = new NSInteger[3]{
        AVCaptureExposureModeLocked,
        AVCaptureExposureModeAutoExpose,
        AVCaptureExposureModeContinuousAutoExposure
    };
    NSInteger mode = availValues[ value % 3 ];
    if([controller.captureDevice lockForConfiguration:nil]) {
        if( [controller.captureDevice isExposureModeSupported:(AVCaptureExposureMode)mode] ) {
            [controller.captureDevice setExposureMode:(AVCaptureExposureMode)mode];
        } else {
            NSLog(@"Error: Cannot set exposure mode %ld", (long)mode );
        }
        [controller.captureDevice unlockForConfiguration];
    }
}


// TODO: Consider renaming to IsExposureCompensationModeSupported...
extern "C" bool UnityIsExposureModeSupported( const char* spcapture ) {
    void *pcapture = str2ptr(spcapture);
    CameraCaptureController* controller = (__bridge CameraCaptureController*)(void*)pcapture;
    return false;
}


// Kristian: Added this method to get the properties of the iOS camera in a similar format
// as is returned as the format given by the Native Android plugin.
extern "C" const char* UnityCaptureGetProperties( const char* spcapture ) {
    void *pcapture = str2ptr(spcapture);
    CameraCaptureController* controller = (__bridge CameraCaptureController*)(void*)pcapture;
    
    NSMutableDictionary* dictRet = [NSMutableDictionary dictionary];

    
    // --------------------------------
    AVCaptureFocusMode focusMode = controller.captureDevice.focusMode;
    NSString* pValue = @"null";
    if( focusMode == AVCaptureFocusModeLocked ) {
        pValue = @"Locked";
    } else if( focusMode == AVCaptureFocusModeAutoFocus ) {
        pValue = @"AutoFocus";
    } else if( focusMode == AVCaptureFocusModeContinuousAutoFocus ) {
        pValue = @"ContinuousAutoFocus";
    }
    NSMutableArray* arr_avail = [NSMutableArray array];
    if( [controller.captureDevice isFocusModeSupported:AVCaptureFocusModeLocked] ) {
        [arr_avail addObject:@"Locked"];
    }
    if( [controller.captureDevice isFocusModeSupported:AVCaptureFocusModeAutoFocus] ) {
        [arr_avail addObject:@"AutoFocus"];
    }
    if( [controller.captureDevice isFocusModeSupported:AVCaptureFocusModeContinuousAutoFocus] ) {
        [arr_avail addObject:@"ContinuousAutoFocus"];
    }
    NSDictionary* prop = [[NSDictionary alloc] initWithObjectsAndKeys:
            @"FocusMode", @"name",
            pValue, @"value",
            arr_avail, @"avail",
            nil];
    [dictRet setValue:prop forKey:@"FocusMode"];
    
    
    // --------------------------------
    AVCaptureFlashMode flashMode = controller.captureDevice.flashMode;
    pValue = @"null";
    if( flashMode == AVCaptureFlashModeOff ) {
        pValue = @"Off";
    } else if( flashMode == AVCaptureFlashModeOn ) {
        pValue = @"On";
    } else if( flashMode == AVCaptureFlashModeAuto ) {
        pValue = @"Auto";
    }
    arr_avail = [NSMutableArray array];
    if( [controller.captureDevice isFlashModeSupported:AVCaptureFlashModeOff] ) {
        [arr_avail addObject:@"Off"];
    }
    if( [controller.captureDevice isFlashModeSupported:AVCaptureFlashModeOn] ) {
        [arr_avail addObject:@"On"];
    }
    if( [controller.captureDevice isFlashModeSupported:AVCaptureFlashModeAuto] ) {
        [arr_avail addObject:@"Auto"];
    }
    prop = [[NSDictionary alloc] initWithObjectsAndKeys:
                                @"FlashMode", @"name",
                                pValue, @"value",
                                arr_avail, @"avail",
                                nil];
    [dictRet setValue:prop forKey:@"FlashMode"];
  
    
   
    
    // --------------------------------
    AVCaptureTorchMode torchMode = controller.captureDevice.torchMode;
    pValue = @"null";
    if( torchMode == AVCaptureTorchModeOff ) {
        pValue = @"Off";
    } else if( torchMode == AVCaptureTorchModeOn ) {
        pValue = @"On";
    } else if( torchMode == AVCaptureTorchModeAuto ) {
        pValue = @"Auto";
    }
    arr_avail = [NSMutableArray array];
    if( [controller.captureDevice isTorchModeSupported:AVCaptureTorchModeOff] ) {
        [arr_avail addObject:@"Off"];
    }
    if( [controller.captureDevice isTorchModeSupported:AVCaptureTorchModeOn] ) {
        [arr_avail addObject:@"On"];
    }
    if( [controller.captureDevice isTorchModeSupported:AVCaptureTorchModeAuto] ) {
        [arr_avail addObject:@"Auto"];
    }
    prop = [[NSDictionary alloc] initWithObjectsAndKeys:
            @"TorchMode", @"name",
            pValue, @"value",
            arr_avail, @"avail",
            nil];
    [dictRet setValue:prop forKey:@"TorchMode"];
    
    
    
    // --------------------------------
    AVCaptureExposureMode exposureMode = controller.captureDevice.exposureMode;
    pValue = @"null";
    if( exposureMode == AVCaptureExposureModeLocked ) {
        pValue = @"Locked";
    } else if( exposureMode == AVCaptureExposureModeAutoExpose ) {
        pValue = @"AutoExpose";
    } else if( exposureMode == AVCaptureExposureModeContinuousAutoExposure ) {
        pValue = @"ContinuousAutoExposure";
    }
    arr_avail = [NSMutableArray array];
    if( [controller.captureDevice isExposureModeSupported:AVCaptureExposureModeLocked] ) {
        [arr_avail addObject:@"Locked"];
    }
    if( [controller.captureDevice isExposureModeSupported:AVCaptureExposureModeAutoExpose] ) {
        [arr_avail addObject:@"AutoExpose"];
    }
    if( [controller.captureDevice isExposureModeSupported:AVCaptureExposureModeContinuousAutoExposure] ) {
        [arr_avail addObject:@"ContinuousAutoExposure"];
    }
    prop = [[NSDictionary alloc] initWithObjectsAndKeys:
            @"ExposureMode", @"name",
            pValue, @"value",
            arr_avail, @"avail",
            nil];
    [dictRet setValue:prop forKey:@"ExposureMode"];
    
    
    // --------------------------------
    pValue = @"null";
    if( controller.captureDevice.focusPointOfInterestSupported ) {
        pValue = NSStringFromCGPoint(controller.captureDevice.focusPointOfInterest);
    } else {
    }
    arr_avail = [NSMutableArray array];
    if( controller.captureDevice.focusPointOfInterestSupported ) {
        [arr_avail addObject:@"Any Point"];
    }
    prop = [[NSDictionary alloc] initWithObjectsAndKeys:
            @"FocusPointOfInterest", @"name",
            pValue, @"value",
            arr_avail, @"avail",
            nil];
    [dictRet setValue:prop forKey:@"FocusPointOfInterest"];
    
    
    // --------------------------------
    pValue = @"null";
    if( controller.captureDevice.exposurePointOfInterestSupported ) {
        pValue = NSStringFromCGPoint(controller.captureDevice.exposurePointOfInterest);
    } else {
    }
    arr_avail = [NSMutableArray array];
    if( controller.captureDevice.exposurePointOfInterestSupported ) {
        [arr_avail addObject:@"Any Point"];
    }
    prop = [[NSDictionary alloc] initWithObjectsAndKeys:
            @"ExposurePointOfInterest", @"name",
            pValue, @"value",
            arr_avail, @"avail",
            nil];
    [dictRet setValue:prop forKey:@"ExposurePointOfInterest"];

    
    // --------------------------------
    pValue = @"null";
    if( controller.captureDevice.smoothAutoFocusSupported ) {
        pValue = [controller.captureDevice isSmoothAutoFocusEnabled] ? @"On" : @"Off";
    } else {
    }
    arr_avail = [NSMutableArray array];
    if( controller.captureDevice.smoothAutoFocusSupported ) {
        [arr_avail addObject:@"On"];
        [arr_avail addObject:@"Off"];
    }
    prop = [[NSDictionary alloc] initWithObjectsAndKeys:
            @"SmoothAutoFocus", @"name",
            pValue, @"value",
            arr_avail, @"avail",
            nil];
    [dictRet setValue:prop forKey:@"SmoothAutoFocus"];

    
    
    // --------------------------------
    AVCaptureAutoFocusRangeRestriction rangeMode = controller.captureDevice.autoFocusRangeRestriction;
    pValue = @"null";
    if( rangeMode == AVCaptureAutoFocusRangeRestrictionNone ) {
        pValue = @"None";
    } else if( rangeMode == AVCaptureAutoFocusRangeRestrictionNear ) {
        pValue = @"Near";
    } else if( rangeMode == AVCaptureAutoFocusRangeRestrictionFar ) {
        pValue = @"Far";
    }
    arr_avail = [NSMutableArray array];
    if( [controller.captureDevice isAutoFocusRangeRestrictionSupported] ) {
        [arr_avail addObject:@"None"];
        [arr_avail addObject:@"Near"];
        [arr_avail addObject:@"Far"];
    }
    prop = [[NSDictionary alloc] initWithObjectsAndKeys:
            @"AutoFocusRangeRestriction", @"name",
            pValue, @"value",
            arr_avail, @"avail",
            nil];
    [dictRet setValue:prop forKey:@"AutoFocusRangeRestriction"];

    
    // --------------------------------
    pValue = @"null";
    if( controller.captureDevice.lowLightBoostSupported ) {
        pValue = [controller.captureDevice isLowLightBoostEnabled] ? @"On" : @"Off";
    } else {
    }
    arr_avail = [NSMutableArray array];
    if( controller.captureDevice.lowLightBoostSupported ) {
        [arr_avail addObject:@"On"];
        [arr_avail addObject:@"Off"];
    }
    prop = [[NSDictionary alloc] initWithObjectsAndKeys:
            @"LowLightBoost", @"name",
            pValue, @"value",
            arr_avail, @"avail",
            nil];
    [dictRet setValue:prop forKey:@"LowLightBoost"];
    
    
    
    NSString* strRet = [dictRet CamCap_dictToJsonStr:TRUE];
    
    return MakeStringCopy( [strRet UTF8String] );
}



// -----CameraCaptureKit end ------------


extern "C" void UnityCameraCaptureExtents( void* capture, int* w, int* h) {
    CameraCaptureController* controller = (__bridge CameraCaptureController*)capture;
    *w = controller->_width;
    *h = controller->_height;
}


extern "C" int UnityCameraCaptureGetWidth( const char* spcapture ) {
    void *pcapture = str2ptr(spcapture);
    CameraCaptureController* controller = (__bridge CameraCaptureController*)(void*)pcapture;
    return controller->_width;
}


extern "C" int UnityCameraCaptureGetHeight( const char* spcapture) {
    void *pcapture = str2ptr(spcapture);
    CameraCaptureController* controller = (__bridge CameraCaptureController*)(void*)pcapture;
    return controller->_height;
}


extern "C" void* UnityCameraCaptureReadPngAndStop( const char* spcapture, int* nBytesRet, int w, int h, bool mirrored ) {
    void *pcapture = str2ptr(spcapture);
    CameraCaptureController* controller = (__bridge CameraCaptureController*)(void*)pcapture;

    UIImageOrientation orient = UIImageOrientationUp;
    bool flipWH = false;
    float scaleValue = 1.0f;
    int imirrored = UnityCameraCaptureVerticallyMirrored(pcapture);
    int rot = UnityCameraCaptureVideoRotationDeg(pcapture);
    bool front_device = controller.captureDevice.position == AVCaptureDevicePositionFront;
    
    // mirrored=false;
    // Counter rotate the image.
    // NSLog(@"mirrored = %i ; rot = %i ; front_device = %i ; imirrored=%i", mirrored, rot , front_device, imirrored  );
    if( rot == 0 ) { // landscape
        if( front_device ) {
            orient = UIImageOrientationUpMirrored;
        } else {
            orient = UIImageOrientationUp;
        }
        
    } else if( rot == 90 ) { // portrait
        flipWH = true;
        if( front_device ) {
            orient = UIImageOrientationLeftMirrored;
        } else {
            orient = UIImageOrientationRight;
        }
        
    } else if( rot == 180 ) { // landscape
        if( front_device ) {
            orient = UIImageOrientationDownMirrored;
        } else {
            orient = UIImageOrientationDown;
        }
        
    } else if( rot == 270 ) { // portrait
        flipWH = true;
        if( front_device ) {
            orient = UIImageOrientationRightMirrored;
        } else {
            orient = UIImageOrientationLeft;
        }
        
    }
    
    //
    if( flipWH ) {
        int t=h;
        h = w;
        w = t;
    }
    
    // New method where we try and read into a png buffer
    CVPixelBufferRef pbuf = (CVPixelBufferRef)controller->_cmVideoSampling.cvImageBuffer;
    
    //
    CFDictionaryRef attachments = CMCopyDictionaryOfAttachments(kCFAllocatorDefault, pbuf, kCMAttachmentMode_ShouldPropagate);
    CIImage *ciImage = [[CIImage alloc] initWithCVPixelBuffer:pbuf options:(__bridge NSDictionary *)attachments];
    if (attachments)
        CFRelease(attachments);
    attachments = nil;
    
    //size_t wi = CVPixelBufferGetWidth(pbuf);
    //size_t he = CVPixelBufferGetHeight(pbuf);
    UIImage* srcImg = [[UIImage alloc] initWithCIImage:ciImage scale:scaleValue orientation:orient];
    ciImage = nil; // release the ciImage
    
    
    UIGraphicsBeginImageContext(CGSizeMake(w,h));
    CGContextRef context = UIGraphicsGetCurrentContext();
    
    
    [srcImg drawInRect:CGRectMake(0, 0, w, h)];
    
    UIImage *dstImg = UIGraphicsGetImageFromCurrentImageContext();
    UIGraphicsEndImageContext();
    srcImg = nil;
    
    NSData* data = UIImagePNGRepresentation(dstImg);
    
    int len = (int)[data length];
    *nBytesRet = len;
    
    void* dataPtr = calloc( len, sizeof(uint8_t) );
    memcpy(dataPtr, [data bytes], len);
    
    [controller stop];
    
    return (void*)dataPtr;
}


extern "C" int UnityCameraCaptureReadPixels(void* capture, void* dataPtr, int w, int h) {
    CameraCaptureController* controller = (__bridge CameraCaptureController*)capture;
    return (int)0;
}


extern "C" void UnityCameraCaptureReadToMemory(void* capture, void* dst_, int w, int h) {
    CameraCaptureController* controller = (__bridge CameraCaptureController*)capture;
    assert(w == controller->_width && h == controller->_height);
    
    CVPixelBufferRef pbuf = (CVPixelBufferRef)controller->_cmVideoSampling.cvImageBuffer;
    
    const size_t srcRowSize	= CVPixelBufferGetBytesPerRow(pbuf);
    const size_t dstRowSize	= w*sizeof(uint32_t);
    const size_t bufSize	= srcRowSize * h;
    
    // while not the best way memory-wise, we want to minimize stalling
    uint8_t* tmpMem = (uint8_t*)::malloc(bufSize);
    CVPixelBufferLockBaseAddress(pbuf, kCVPixelBufferLock_ReadOnly);
    {
        uint8_t * addr = (uint8_t *) CVPixelBufferGetBaseAddress(pbuf);
        ::memcpy(tmpMem, addr, bufSize);
    }
    CVPixelBufferUnlockBaseAddress(pbuf, kCVPixelBufferLock_ReadOnly);
    
    uint8_t* dst = (uint8_t*)dst_;
    uint8_t* src = tmpMem + (h - 1)*srcRowSize;
    for( int i = 0, n = h ; i < n ; ++i)
    {
        ::memcpy(dst, src, dstRowSize);
        dst += dstRowSize;
        src -= srcRowSize;
    }
    ::free(tmpMem);
}



extern "C" int UnityCameraCaptureVideoRotationDeg(void* capture) {
    CameraCaptureController* controller = (__bridge CameraCaptureController*)capture;
    // default orientation is laandscape
    switch(UnityCurrentOrientation()) {
        case portrait:				return 90;
        case portraitUpsideDown:	return 270;
        case landscapeLeft:			return controller.captureDevice.position == AVCaptureDevicePositionFront ? 180 : 0;
        case landscapeRight:		return controller.captureDevice.position == AVCaptureDevicePositionFront ? 0 : 180;
            
        default:					assert(false && "bad orientation returned from UnityCurrentOrientation()");	break;
    }
    return 0;
}


extern "C" int UnityCameraCaptureVerticallyMirrored(void* capture) {
    CameraCaptureController* controller = (__bridge CameraCaptureController*)capture;
    return IsCVTextureFlipped(controller->_cmVideoSampling.cvTextureCacheTexture);
}


extern "C" void UnitySetNextSessionIsStillImage( ) {
    NextCaptureSessionIsStillImage = YES;
}


extern "C" void UnitySetStatusBarHidden( bool value ) {
    NSLog(@"Setting statusbar hidden : %d" , value );
    [[UIApplication sharedApplication] setStatusBarHidden:(value?YES:NO) withAnimation:UIStatusBarAnimationNone];
}





