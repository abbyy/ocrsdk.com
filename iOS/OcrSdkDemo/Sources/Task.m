#import "Task.h"

@interface Task (Private)

- (TaskStatus)statusFromString:(NSString*)statusString;

@end

@implementation Task

@synthesize ID;
@synthesize status;
@synthesize downloadURL;

- (id)initWithData:(NSData*)data
{
	self = [super init];
	
	if (self) {
		NSXMLParser* parser = [[NSXMLParser alloc] initWithData:data];
		[parser setDelegate:self];
		
		if (![parser parse]) {
			self = nil;
		}
	}
	
	return self;
}

- (BOOL)isActive
{
	return (self.status == Queued || self.status == InProgress);
}

- (void)parser:(NSXMLParser *)parser didStartElement:(NSString *)elementName namespaceURI:(NSString *)namespaceURI qualifiedName:(NSString *)qName attributes:(NSDictionary *)attributeDict
{
	if ([elementName isEqualToString:@"task"]) {
		self.ID = [attributeDict valueForKey:@"id"];
		self.status = [self statusFromString:[attributeDict valueForKey:@"status"]];
		
		if (self.status == Completed) {
			NSString* str = [attributeDict valueForKey:@"resultUrl"];
			NSURL* url = [NSURL URLWithString:str];
			self.downloadURL = url;
		}
	} else if ([elementName isEqualToString:@"error"]) {
		// TODO: handle errors here
	}
}

- (TaskStatus)statusFromString:(NSString*)statusString
{
	if ([statusString isEqualToString:@"Submitted"])
		return Submitted;
	else if ([statusString isEqualToString:@"Queued"])
		return Queued;
	else if ([statusString isEqualToString:@"InProgress"])
		return InProgress;
	else if ([statusString isEqualToString:@"Completed"])
		return Completed;
	else if ([statusString isEqualToString:@"ProcessingFailed"])
		return ProcessingFailed;
	else if ([statusString isEqualToString:@"Deleted"])
		return Deleted;
	else if ([statusString isEqualToString:@"NotEnoughCredits"])
		return NotEnoughCredits;
	else
		return Unknown;
}

@end
