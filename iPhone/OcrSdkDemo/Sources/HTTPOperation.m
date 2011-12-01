#import "HTTPOperation.h"

@implementation HTTPOperation

@synthesize request = _request;
@synthesize recievedData = _recievedData;
@synthesize error = _error;

@synthesize isCanceled = _canceled;

@synthesize authenticationDelegate;

- (id)initWithRequest:(NSURLRequest*)request target:(id)target finishedAction:(SEL)action
{
	self = [super init];
	
	if (self) {
		self.request = request;
	
		_target = target;
		_targetTherad = [NSThread currentThread];
		
		_action = action;
		
		
		_recievedData = [NSMutableData data];
	}
	
	return self;
}

- (void)start
{
	NSParameterAssert(self.request);
	
	_canceled = NO;
	
	_connection = [[NSURLConnection alloc] initWithRequest:self.request delegate:self startImmediately:NO];
	
	[_connection scheduleInRunLoop:[NSRunLoop mainRunLoop] forMode:NSDefaultRunLoopMode];
	
	[_connection start];
}

- (void)cancel
{
	_canceled = YES;
	[_connection cancel];
}

- (void)finishWithError:(NSError*)error
{	
	_error = error;
	
	if (!self.isCanceled) {
		[_target performSelector:_action onThread:_targetTherad withObject:self waitUntilDone:NO];
	}
}

#pragma mark - NSURLConnectionDelegate implementation

- (BOOL)connection:(NSURLConnection *)connection canAuthenticateAgainstProtectionSpace:(NSURLProtectionSpace *)protectionSpace
{
    if (self.authenticationDelegate != nil) {
        return [self.authenticationDelegate httpOperation:self canAuthenticateAgainstProtectionSpace:protectionSpace];
    }
	
    return NO;
}

- (void)connection:(NSURLConnection *)connection didReceiveAuthenticationChallenge:(NSURLAuthenticationChallenge *)challenge
{
	if (self.authenticationDelegate != nil) {
        [self.authenticationDelegate httpOperation:self didReceiveAuthenticationChallenge:challenge];
    } else {
        if ([challenge previousFailureCount] == 0) {
            [[challenge sender] continueWithoutCredentialForAuthenticationChallenge:challenge];
        } else {
            [[challenge sender] cancelAuthenticationChallenge:challenge];
        }
    }
}

- (void)connection:(NSURLConnection *)connection didFailWithError:(NSError *)error
{
	[self finishWithError:error];
}

#pragma mark - NSURLConnectionDataDelegate implementation

- (void)connection:(NSURLConnection *)connection didReceiveResponse:(NSURLResponse *)response
{
	[_recievedData setLength:0];
}

- (void)connection:(NSURLConnection *)connection didReceiveData:(NSData *)data
{
	[_recievedData appendData:data];
}

- (void)connectionDidFinishLoading:(NSURLConnection *)connection
{
	[self finishWithError:nil];
}

@end
