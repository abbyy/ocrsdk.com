#import <UIKit/UIKit.h>

@interface OCRTextViewController : UIViewController

@property (weak, nonatomic) IBOutlet UITextView *textView;

@property (strong, nonatomic) NSString *text;

@end
