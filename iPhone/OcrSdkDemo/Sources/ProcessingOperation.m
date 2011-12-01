#import "ProcessingOperation.h"
#import "Task.h"

@implementation ProcessingOperation

- (void)finishWithError:(NSError*)error
{		
	if (error == nil) {
		Task* task = [[Task alloc] initWithData:self.recievedData];
	
		if ([task isActive]) {
			NSLog(@"Waiting for image processing complete...");
			
			[self performSelector:@selector(start) withObject:nil afterDelay:1];
			
			return;
		}
	}
	
	[super finishWithError:error];
}

@end
