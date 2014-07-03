#import <UIKit/UIKit.h>
#import "Client.h"

@interface RecognitionViewController : UIViewController<ClientDelegate>

@property (weak, nonatomic) IBOutlet UITextView *textView;

@property (weak, nonatomic) IBOutlet UILabel *statusLabel;
@property (weak, nonatomic) IBOutlet UIActivityIndicatorView *statusIndicator;

@end
