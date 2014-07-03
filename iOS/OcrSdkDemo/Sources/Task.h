#import <Foundation/Foundation.h>

typedef enum {
	Unknown = 0, 
	Submitted, 
	Queued, 
	InProgress, 
	Completed, 
	ProcessingFailed, 
	Deleted, 
	NotEnoughCredits
} TaskStatus;

@interface Task : NSObject<NSXMLParserDelegate>

@property (strong, nonatomic) NSString* ID;
@property (nonatomic) TaskStatus status;
@property (strong, nonatomic) NSURL* downloadURL;

- (id)initWithData:(NSData*)data;
- (BOOL)isActive;

@end
