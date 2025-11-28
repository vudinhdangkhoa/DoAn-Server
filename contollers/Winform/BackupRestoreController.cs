using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using server.Models;

namespace server.contollers.Winform
{
    [ApiController]
    [Route("api/[controller]")]
    public class BackupRestoreController : ControllerBase
    {
        private MyDbContext db;
        private IConfiguration _configuration;
        private string backupFolder;
        
        public BackupRestoreController(MyDbContext db, IConfiguration configuration, IWebHostEnvironment env)
        {
            this.db = db;
            this._configuration = configuration;

            // Đọc đường dẫn từ config, nếu không có thì dùng mặc định
            backupFolder = _configuration["BackupSettings:BackupFolder"];
            
            // Nếu không có trong config, dùng đường dẫn tuyệt đối mặc định
            if (string.IsNullOrEmpty(backupFolder))
            {
                backupFolder = Path.Combine(env.ContentRootPath, "SQLBackups");
            }

            // Tạo thư mục nếu chưa tồn tại
            if (!Directory.Exists(backupFolder))
            {
                Directory.CreateDirectory(backupFolder);
            }
        }

        [HttpGet("backup")]
        public async Task<IActionResult> BackupDatabase()
        {
            string dbName = db.Database.GetDbConnection().Database;
            string fileName = $"{dbName}_{DateTime.Now:yyyyMMddHHmmss}.bak";
            string backupPath = Path.Combine(backupFolder, fileName);

            try
            {
                // 1. Backup database với đường dẫn tuyệt đối
                string sqlCommand = $"BACKUP DATABASE [{dbName}] TO DISK = @path WITH FORMAT, INIT";
                await db.Database.ExecuteSqlRawAsync(sqlCommand, new SqlParameter("@path", backupPath));

                // 2. Kiểm tra file có tồn tại không
                if (!System.IO.File.Exists(backupPath))
                {
                    return StatusCode(500, "Backup thành công nhưng không tìm thấy file.");
                }

                // 3. Trả file về client
                var stream = new FileStream(backupPath, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
                
                return File(stream, "application/octet-stream", fileName);
            }
            catch (SqlException sqlEx)
            {
                return StatusCode(500, $"Lỗi SQL Server: {sqlEx.Message}\nĐường dẫn: {backupPath}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi Backup: {ex.Message}\nĐường dẫn: {backupPath}");
            }
        }

        [HttpPost("restore")]
        [RequestSizeLimit(long.MaxValue)]
        public async Task<IActionResult> RestoreDatabase(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File không hợp lệ.");

            string dbName = db.Database.GetDbConnection().Database;
            string restorePath = Path.Combine(backupFolder, "restore_temp.bak");

            try
            {
                // 1. Lưu file upload vào đường dẫn tuyệt đối
                using (var stream = new FileStream(restorePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // 2. Kiểm tra file đã được lưu
                if (!System.IO.File.Exists(restorePath))
                {
                    return StatusCode(500, "Không thể lưu file backup.");
                }

                // 3. Thực hiện Restore
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                
                var builder = new SqlConnectionStringBuilder(connectionString);
                builder.InitialCatalog = "master";
                builder.ConnectTimeout = 300; // Tăng timeout lên 5 phút
                
                using (var connection = new SqlConnection(builder.ConnectionString))
                {
                    await connection.OpenAsync();

                    // Restore với đường dẫn tuyệt đối
                    string sql = $@"
                        ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                        RESTORE DATABASE [{dbName}] FROM DISK = N'{restorePath}' WITH REPLACE;
                        ALTER DATABASE [{dbName}] SET MULTI_USER;
                    ";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.CommandTimeout = 300; // 5 phút
                        await command.ExecuteNonQueryAsync();
                    }
                }

                // 4. Xóa file tạm
                if (System.IO.File.Exists(restorePath))
                {
                    System.IO.File.Delete(restorePath);
                }

                return Ok("Khôi phục dữ liệu thành công.");
            }
            catch (SqlException sqlEx)
            {
                // Xóa file tạm nếu lỗi
                if (System.IO.File.Exists(restorePath))
                {
                    System.IO.File.Delete(restorePath);
                }
                return StatusCode(500, $"Lỗi SQL Server: {sqlEx.Message}\nĐường dẫn: {restorePath}");
            }
            catch (Exception ex)
            {
                // Xóa file tạm nếu lỗi
                if (System.IO.File.Exists(restorePath))
                {
                    System.IO.File.Delete(restorePath);
                }
                return StatusCode(500, $"Lỗi Restore: {ex.Message}");
            }
        }

        // API kiểm tra cấu hình (có thể xóa sau khi test xong)
        [HttpGet("test-config")]
        public IActionResult TestConfig()
        {
            return Ok(new 
            { 
                BackupFolder = backupFolder,
                FolderExists = Directory.Exists(backupFolder),
                DatabaseName = db.Database.GetDbConnection().Database
            });
        }


    }
}