#import <Foundation/Foundation.h>

@protocol HTTPOperationAuthenticationDelegate;

@interface HTTPOperation : NSObject<NSURLConnectionDelegate, NSURLConnectionDataDelegate> {
	id _target;
	SEL _action;
	NSThread* _targetThread;
	NSMutableData* _receivedData;
	
	NSURLConnection* _connection;
	
	BOOL _canceled;
}

@property (strong) NSURLRequest* request;
@property (strong, readonly) NSData* receivedData;
@property (strong, readonly) NSError* error;

@property (readonly) BOOL isCanceled;

@property (assign) id<HTTPOperationAuthenticationDelegate> authenticationDelegate;

- (id)initWithRequest:(NSURLRequest*)request target:(id)target finishedAction:(SEL)action;

- (void)start;

- (void)cancel;

- (void)finishWithError:(NSError *)error;

@end

@protocol HTTPOperationAuthenticationDelegate <NSObject>

@required

- (BOOL)httpOperation:(HTTPOperation*)operation canAuthenticateAgainstProtectionSpace:(NSURLProtectionSpace *)protectionSpace;
- (void)httpOperation:(HTTPOperation*)operation didReceiveAuthenticationChallenge:(NSURLAuthenticationChallenge *)challenge;

@end