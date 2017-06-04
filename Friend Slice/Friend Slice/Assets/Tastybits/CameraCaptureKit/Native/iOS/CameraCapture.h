#pragma once

#import <AVFoundation/AVFoundation.h>

@interface CameraCaptureController : NSObject <AVCaptureVideoDataOutputSampleBufferDelegate,AVCaptureMetadataOutputObjectsDelegate>

- (bool)initCapture:(AVCaptureDevice*)device width:(int)width height:(int)height fps:(float)fps;
- (void)captureOutput:(AVCaptureOutput*)captureOutput didOutputSampleBuffer:(CMSampleBufferRef)sampleBuffer fromConnection:(AVCaptureConnection*)connection;
- (void)captureOutput:(AVCaptureOutput *)captureOutput didOutputMetadataObjects:(NSArray *)metadataObjects fromConnection:(AVCaptureConnection *)connection;
-(void) captureStillImage; //KRM

- (void)start;
- (void)pause;
- (void)stop;

@property (nonatomic, retain) AVCaptureDevice*			captureDevice;
@property (nonatomic, retain) AVCaptureSession*			captureSession;
@property (nonatomic, retain) AVCaptureDeviceInput*		captureInput;
@property (nonatomic, retain) AVCaptureVideoDataOutput*	captureOutput;
@property (nonatomic, retain) AVCaptureStillImageOutput*	captureStillImageOutput;

@property (nonatomic, retain) AVCaptureMetadataOutput*	captureQROutput;

@property (nonatomic, retain) NSString*                 captureStillImageCB;
@property (nonatomic, retain) NSString*                 firstFrameCallback;

// override these two for custom preset/fps selection
// they will be called on inited capture
- (NSString*)pickPresetFromWidth:(int)w height:(int)h;
- (AVFrameRateRange*)pickFrameRateRange:(float)fps;
- (void) setCaptureStillCallback:(NSString*)value;

- (void) fireCaptureStillCallback;

- (void) setFlash:(bool)val;

- (void) fireFirstFrame;

@end
