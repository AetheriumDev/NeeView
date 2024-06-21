﻿namespace NeeView
{
    [DocumentableBaseClass(typeof(PageAccessor))]
    public record class ViewPageAccessor : PageAccessor
    {
        public ViewPageAccessor(Page page) : base(page)
        {
        }

        [WordNodeMember]
        public double Width
        {
            get
            {
                if (this.Source.Content is BitmapPageContent bitmapContent && bitmapContent.PictureInfo != null)
                {
                    return bitmapContent.PictureInfo.OriginalSize.Width;
                }
                else
                {
                    return 0.0;
                }
            }
        }

        [WordNodeMember]
        public double Height
        {
            get
            {
                if (this.Source.Content is BitmapPageContent bitmapContent && bitmapContent.PictureInfo != null)
                {
                    return bitmapContent.PictureInfo.OriginalSize.Height;
                }
                else
                {
                    return 0.0;
                }
            }
        }


        [WordNodeMember]
        public PageAccessor GetPageAccessor() => new PageAccessor(Source);
    }
}
