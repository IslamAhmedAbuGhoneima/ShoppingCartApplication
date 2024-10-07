﻿namespace ShoppingCart.Web.Helpers
{
    public enum Roles
    {
        Admin,
        Editor,
        Customer,

    }
    public static class Application
    {
        public static string  ImageUpload(IWebHostEnvironment webHost, IFormFile ImageUrl)
        {
            string rootPath = webHost.WebRootPath;

            string fileName = Guid.NewGuid().ToString();
            string fileExtension = Path.GetExtension(ImageUrl.FileName);
            string imagePath = fileName + fileExtension;
            string path = Path.Combine(rootPath,
                @"img\products\" + imagePath
                );

            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                ImageUrl.CopyTo(fileStream);
            }

            return $@"img\products\{imagePath}";
        }

    }
}
