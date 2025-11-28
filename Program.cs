using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
using Microsoft.VisualBasic;
using server;
using server.Models;
using server.Services;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddHttpContextAccessor();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    option =>
    {

        option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Nhập token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "bearer"
        });
        option.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type=ReferenceType.SecurityScheme,
                        Id="Bearer"
                    },
                    Scheme="oauth2",
                    Name="Bearer",
                    In = ParameterLocation.Header
                },
                new string[]{}
            }
        });
    }
);


//Thêm DbContext
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));




builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder.WithOrigins("http://localhost:3000")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();

    });
});

//Thêm các service
builder.Services.AddScoped<JWT_Services>();
builder.Services.AddScoped<Google_Services>();
builder.Services.AddScoped<Mail_Services>();
builder.Services.AddScoped<MoMo_Services>();
builder.Services.AddScoped<VNPay_Services>();

// Add HangFire Services
builder.Services.AddScoped<HangFire_Services>();
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

//add the HangFire server
builder.Services.AddHangfireServer();


builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Lấy token từ query string nếu có
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/hubs")))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
}).AddGoogle(

    option =>
    {
        option.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        option.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    }
);


builder.Services.AddAuthorization();
builder.Services.AddControllers();

builder.Services.Configure<FormOptions>(x =>
{
    x.ValueLengthLimit = int.MaxValue;
    x.MultipartBodyLengthLimit = long.MaxValue; // Cho phép file cực lớn
});


var app = builder.Build();

app.UseForwardedHeaders();

try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();

        if(dbContext.KhuyenMais.Any() == false)
        {
            var khuyenMai = new KhuyenMai
            {
                TenKhuyenMai = "Khuyến mãi back to school",
                PhanTramKhuyenMai = 0.1
            };
            dbContext.KhuyenMais.Add(khuyenMai);
            dbContext.SaveChanges();
        }

        if (dbContext.NhaCungCaps.Any() == false)
        {
            var nhaCungCaps = new List<NhaCungCap>
            {
                new NhaCungCap { TenNhaCungCap = "Công ty TNHH Mỹ Thuật Vạn Phát", Sdt = "0123456789" },
                new NhaCungCap { TenNhaCungCap = "Cửa Hàng Vật Tư Nghệ Thuật An Thanh", Sdt = "0987654321" },
                new NhaCungCap { TenNhaCungCap = "Nhà Sách Nghệ Thuật Mỹ Tâm", Sdt = "0912345678" }
            };
            dbContext.NhaCungCaps.AddRange(nhaCungCaps);
            dbContext.SaveChanges();
        }

        if (dbContext.LoaiHocCus.Any() == false)
        {
            
            var loaiHocCus = new List<LoaiHocCu>
            {
                new LoaiHocCu { TenLoai = "Bút chì" },
                new LoaiHocCu { TenLoai = "Màu nước" },
                new LoaiHocCu { TenLoai = "Màu Acrylic" },
                new LoaiHocCu { TenLoai = "Sơn dầu" },
                new LoaiHocCu { TenLoai = "Cọ vẽ" },
                new LoaiHocCu { TenLoai = "Giấy vẽ" },
                new LoaiHocCu { TenLoai = "Khung vẽ" },
                new LoaiHocCu { TenLoai = "Bảng màu" }
            };
            dbContext.LoaiHocCus.AddRange(loaiHocCus);
            dbContext.SaveChanges();

        }
        if(dbContext.HocCus.Any() == false)
        {
            var hocCus = new List<HocCu>
            {
                new HocCu { TenHocCu="Bút chì 2B", IdLoaiHocCu=1, DonViTinh="Cái", GiaBan=5000, SoLuong=100 },
                new HocCu { TenHocCu="Bút chì 4B", IdLoaiHocCu=1, DonViTinh="Cái", GiaBan=5000, SoLuong=100 },
                new HocCu { TenHocCu="Màu nước 12 màu", IdLoaiHocCu=2, DonViTinh="Hộp", GiaBan=60000, SoLuong=50 },
                new HocCu { TenHocCu="Màu Acrylic 24 màu", IdLoaiHocCu=3, DonViTinh="Hộp", GiaBan=150000, SoLuong=30 },
                new HocCu { TenHocCu="Sơn dầu 12 màu", IdLoaiHocCu=4, DonViTinh="Hộp", GiaBan=200000, SoLuong=20 },
                new HocCu { TenHocCu="Cọ vẽ size 6", IdLoaiHocCu=5, DonViTinh="Cái", GiaBan=30000, SoLuong=80 },
                new HocCu { TenHocCu="Giấy vẽ A4", IdLoaiHocCu=6, DonViTinh="Tờ", GiaBan=2000, SoLuong=200 },
                new HocCu { TenHocCu="Khung vẽ 30x40cm", IdLoaiHocCu=7, DonViTinh="Cái", GiaBan=50000, SoLuong=40 },
                new HocCu { TenHocCu="Bảng màu gỗ", IdLoaiHocCu=8, DonViTinh="Cái", GiaBan=80000, SoLuong=25 }
            };
            dbContext.HocCus.AddRange(hocCus);
            dbContext.SaveChanges();
        }

        if (dbContext.Quyens.Any() == false)
        {
            var adminRole = new Quyen
            {
                TenQuyen = DungChung.adminRole
            };
            var nhanVienKhoRole = new Quyen
            {
                TenQuyen = DungChung.nhanVienKhoRole
            };
            var nhanVienHocVuRole = new Quyen
            {
                TenQuyen = DungChung.nhanVienHocVuRole
            };
            dbContext.Quyens.AddRange(adminRole, nhanVienKhoRole, nhanVienHocVuRole);
            dbContext.SaveChanges();
        }

        if (dbContext.PhuHuynhs.Any() == false)
        {
            var user = new User
            {
                Mail = "khachvanglai",
                MatKhau="khachvanglai",
                IsHocVien= true
            };
            dbContext.Users.Add(user);
            dbContext.SaveChanges();
            var phuHuynh = new PhuHuynh
            {
                TenPh = "Khách Vãng Lai",
                UserId = user.UserId,
                Sdt = "0000000000",
                GioiTinh= "Nam",
                NgaySinh= DateOnly.FromDateTime( new DateTime(2000,1,1))
                
            };
            dbContext.PhuHuynhs.Add(phuHuynh);
            dbContext.SaveChanges();
        }

        if (dbContext.NhanViens.Any() == false)
        {
            var user = new User
            {
                Mail = "admin@example.com",
                MatKhau = "Admin@123",
                IsHocVien = false
            };
            dbContext.Users.Add(user);
            dbContext.SaveChanges();

            var adminQuyen = dbContext.Quyens.FirstOrDefault(q => q.TenQuyen == DungChung.adminRole);
            if (adminQuyen != null)
            {
                var staff = new NhanVien
                {
                    TenNv = "Admin",
                    UserId = user.UserId,
                    IdQuyen = adminQuyen.IdQuyen
                };
                dbContext.NhanViens.Add(staff);
                dbContext.SaveChanges();
            }

            var userHocVu = new User
            {
                Mail = "hocvu@example.com",
                MatKhau = "HocVu@123", IsHocVien = false
            };

            dbContext.Users.Add(userHocVu);
            dbContext.SaveChanges();
            var hocVuQuyen = dbContext.Quyens.FirstOrDefault(q => q.TenQuyen == DungChung.nhanVienHocVuRole);
            if (hocVuQuyen != null)
            {
                var staff = new NhanVien
                {
                    TenNv = "Nguyễn Văn A",
                    UserId = userHocVu.UserId,
                    IdQuyen = hocVuQuyen.IdQuyen
                };
                dbContext.NhanViens.Add(staff); dbContext.SaveChanges();
            }

            var userKho = new User
            {
                Mail = "kho@example.com", MatKhau = "Kho@123",
                IsHocVien = false
            };
            dbContext.Users.Add(userKho);
            dbContext.SaveChanges();
            var khoQuyen = dbContext.Quyens.FirstOrDefault(q => q.TenQuyen == DungChung.nhanVienKhoRole);
            if (khoQuyen != null)
            {
                var staff = new NhanVien
                {
                    TenNv = "Nguyễn Thị B",
                    UserId = userKho.UserId,
                    IdQuyen = khoQuyen.IdQuyen
                };                dbContext.NhanViens.Add(staff);
                dbContext.SaveChanges();
            }
        }
        if (!dbContext.ChuyenMons.Any())
        {
            var chuyenMons = new List<ChuyenMon>
            {
                new ChuyenMon
                {
                    TenChuyenMon = "Mỹ thuật thiếu nhi",
                    MoTa = "Tại PPA, hội họa là ngôn ngữ đầu đời của trẻ. Khi ngôn từ chưa đủ để diễn đạt, những nét vẽ ngây ngô chính là cửa sổ tâm hồn, giúp bé tự do khám phá và thể hiện thế giới quan sinh động. Khóa học không chỉ dạy vẽ mà còn nuôi dưỡng tư duy sáng tạo, chỉ số cảm xúc (EQ) và khả năng cảm thụ cái đẹp từ sớm.",
                    HinhAnh = "MyThuatThieuNhi.jpg"
                },
                new ChuyenMon
                {
                    TenChuyenMon = "Sơn dầu",
                    MoTa = "Được mệnh danh là 'Vua của các chất liệu', tranh sơn dầu tại PPA mang đến vẻ đẹp của sự vĩnh cửu. Với độ phủ dày, khả năng phối màu uyển chuyển và độ bền vượt thời gian, học viên sẽ được trải nghiệm sự mê hoặc của những lớp màu chồng chất, tạo nên những tác phẩm có chiều sâu và sự lôi cuốn đầy ma lực.",
                    HinhAnh = "SonDau.jpg"
                },
                new ChuyenMon
                {
                    TenChuyenMon = "Màu nước",
                    MoTa = "Vẻ đẹp của màu nước nằm ở sự trong trẻo, loang màu ngẫu hứng và đầy chất thơ. Đây là bộ môn nghệ thuật của sự tinh tế, nơi người vẽ học cách 'chơi' đùa cùng nước và sắc màu. Tại PPA, bạn sẽ học cách kiểm soát sự loang chảy để tạo ra những bức tranh nhẹ nhàng, bay bổng nhưng vẫn đầy ắp cảm xúc.",
                    HinhAnh = "MauNuoc.jpg"
                },
                new ChuyenMon
                {
                    TenChuyenMon = "Màu Acrylic",
                    MoTa = "Hiện đại, linh hoạt và rực rỡ - đó là Acrylic. Với ưu điểm khô nhanh, bền màu và độ bám dính tuyệt vời, Acrylic cho phép bạn thỏa sức sáng tạo trên mọi chất liệu từ vải toan, gỗ đến tường. Khóa học tại PPA giúp bạn làm chủ kỹ thuật pha màu để tạo nên những tác phẩm ấn tượng trong thời gian ngắn nhất.",
                    HinhAnh = "MauAcrylic.jpg"
                },
                new ChuyenMon
                {
                    TenChuyenMon = "Hình họa - Vẽ chân dung",
                    MoTa = "Hình họa là chiếc chìa khóa vạn năng mở ra cánh cửa hội họa chuyên nghiệp. Khóa học tại PPA giúp bạn thấu hiểu cấu trúc giải phẫu, tỉ lệ vàng và cách diễn tả khối trong không gian. Từ những nét chì đánh bóng cơ bản, bạn sẽ tự tin lột tả được thần thái và cái 'hồn' sâu sắc của nhân vật trên trang giấy.",
                    HinhAnh = "ChanDung.jpg"
                },
                new ChuyenMon
                {
                    TenChuyenMon = "Luyện thi khối H",
                    MoTa = "PPA đồng hành cùng giấc mơ giảng đường của bạn. Chúng tôi hiểu rằng nền tảng vững chắc phải được xây dựng từ sớm. Lộ trình luyện thi được thiết kế bài bản từ Căn bản đến Nâng cao, sát với cấu trúc đề thi đại học, giúp sĩ tử rèn luyện tư duy bố cục, kỹ năng hình họa và tự tin chinh phục mọi kỳ thi tuyển sinh.",
                    HinhAnh = "LuyenThiKhoiH.jpg"
                },
            };

            await dbContext.ChuyenMons.AddRangeAsync(chuyenMons);
            await dbContext.SaveChangesAsync();
        }
        if (dbContext.PhongHocs.Any() == false)
        {
            var phongHocs = new List<PhongHoc>
            {
                new PhongHoc { TenPhong = "Phòng 1",  },
                new PhongHoc { TenPhong = "Phòng 2",  },
                new PhongHoc { TenPhong = "Phòng 3",  },
                new PhongHoc { TenPhong = "Phòng 4",  },
                new PhongHoc { TenPhong = "Phòng 5",  }
            };
            dbContext.PhongHocs.AddRange(phongHocs);
            dbContext.SaveChanges();
        }
        if (!dbContext.KhoaHocs.Any())
        {
            var khoaHocs = new List<KhoaHoc>
            {
                // ============================================================
                // 1. MỸ THUẬT THIẾU NHI
                // ============================================================
                new KhoaHoc
                {
                    IdChuyenMon = 1,
                    TenKhoaHoc = "Mỹ thuật thiếu nhi: Khám phá sắc màu",
                    MoTa = "Khóa học nhập môn dành cho bé từ 6-9 tuổi. Đây là bước đệm quan trọng giúp bé làm quen với nghệ thuật mà không bị gò bó, chuẩn bị nền tảng cho khóa Nâng cao.",
                    MucTieu = "Giúp trẻ nhận biết màu sắc, hình khối cơ bản. Kích thích trí tưởng tượng và rèn luyện sự khéo léo của đôi tay.",
                    LoTrinh = "- Phần 1 (Buổi 1-3): Làm quen với màu sáp, màu nước. Học cách pha màu cơ bản.\n- Phần 2 (Buổi 4-7): Vẽ các hình khối đơn giản, thiên nhiên, động vật qua cái nhìn ngộ nghĩnh.\n- Phần 3 (Buổi 8): Sáng tạo tranh theo chủ đề tự do và hoàn thiện bài tốt nghiệp.",
                    HocPhi = 1600000, // ~200k/buổi
                    SoLuongBuoi = 8,
                    HinhAnh = "thieuNhi1.jpg",
                    NgayTao = DateOnly.FromDateTime(DateTime.Now)
                },
                new KhoaHoc
                {
                    IdChuyenMon = 1,
                    TenKhoaHoc = "Mỹ thuật thiếu nhi: Tư duy sáng tạo",
                    MoTa = "Dành cho các bé đã qua lớp căn bản hoặc có năng khiếu (9-15 tuổi). Học viên sẽ học cách kể chuyện qua tranh và xử lý bố cục phức tạp hơn.",
                    MucTieu = "Phát triển tư duy không gian, bố cục tranh. Biết cách phối màu theo cảm xúc và xây dựng nhân vật có chiều sâu.",
                    LoTrinh = "- Phần 1 (Buổi 1-3): Kiến thức về xa gần (luật phối cảnh), bố cục tranh sinh hoạt.\n- Phần 2 (Buổi 4-10): Thiết kế nhân vật, trang phục và bối cảnh câu chuyện.\n- Phần 3 (Buổi 11-12): Thực hiện dự án truyện tranh ngắn hoặc tranh khổ lớn trưng bày.",
                    HocPhi = 2760000, // ~230k/buổi (cao hơn do kỹ thuật khó hơn)
                    SoLuongBuoi = 12,
                    HinhAnh = "thieuNhi2.jpg",
                    NgayTao = DateOnly.FromDateTime(DateTime.Now)
                },

                // ============================================================
                // 2. SƠN DẦU (OIL PAINTING)
                // ============================================================
                new KhoaHoc
                {
                    IdChuyenMon = 2,
                    TenKhoaHoc = "Sơn dầu nhập môn: Tĩnh vật & Phong cảnh",
                    MoTa = "Làm quen với 'Vua của các chất liệu'. Khóa học hướng dẫn từ cách căng toan, pha dung môi đến hoàn thiện bức tranh đầu tiên. Là nền tảng bắt buộc trước khi học vẽ chân dung.",
                    MucTieu = "Hiểu đặc tính sơn dầu. Nắm vững kỹ thuật lót nền, vờn khối, tả chất liệu vải, gốm, sứ.",
                    LoTrinh = "- Giai đoạn 1 (4 buổi): Lý thuyết về màu, dung môi, cọ. Tập chép tranh tĩnh vật đơn giản.\n- Giai đoạn 2 (6 buổi): Vẽ tĩnh vật phức tạp, luyện tập tả chất liệu (kim loại, thủy tinh).\n- Giai đoạn 3 (6 buổi): Vẽ phong cảnh cơ bản, học về không gian và ánh sáng.",
                    HocPhi = 4000000, // ~250k/buổi (Vật liệu đắt)
                    SoLuongBuoi = 16,
                    HinhAnh = "sonDau1.jpg",
                    NgayTao = DateOnly.FromDateTime(DateTime.Now)
                },
                new KhoaHoc
                {
                    IdChuyenMon = 2,
                    TenKhoaHoc = "Sơn dầu chuyên sâu: Kỹ thuật Cổ điển",
                    MoTa = "Khóa học nâng cao dành cho người đã nắm vững kỹ thuật sơn dầu cơ bản. Đi sâu vào kỹ thuật vẽ nhiều lớp (Glazing) và vẽ đắp dày (Impasto).",
                    MucTieu = "Định hình phong cách cá nhân. Có khả năng chép tranh cổ điển hoặc sáng tác tranh theo ý tưởng riêng.",
                    LoTrinh = "- Phần 1 (Buổi 1-8): Nghiên cứu kỹ thuật Glazing (vẽ láng) để tạo chiều sâu màu sắc.\n- Phần 2 (Buổi 9-16): Kỹ thuật Impasto (vẽ dày) tạo chất cảm mạnh mẽ.\n- Phần 3 (Buổi 17-24): Thực hiện tác phẩm tốt nghiệp khổ lớn (Chân dung hoặc Phong cảnh phức tạp).",
                    HocPhi = 7200000, // ~300k/buổi
                    SoLuongBuoi = 24,
                    HinhAnh = "sonDau2.jpg",
                    NgayTao = DateOnly.FromDateTime(DateTime.Now)
                },

                // ============================================================
                // 3. MÀU NƯỚC (WATERCOLOR)
                // ============================================================
                new KhoaHoc
                {
                    IdChuyenMon = 3,
                    TenKhoaHoc = "Màu nước: Sự kỳ diệu của Nước",
                    MoTa = "Khóa học giúp bạn kiểm soát sự 'đỏng đảnh' của màu nước. Phù hợp cho người mới bắt đầu yêu thích sự nhẹ nhàng, trong trẻo.",
                    MucTieu = "Kiểm soát lượng nước và màu. Thành thạo kỹ thuật loang màu (Wet-on-wet) và vẽ chồng lớp (Wet-on-dry).",
                    LoTrinh = "- Tuần 1-2: Làm quen giấy, cọ. Kỹ thuật loang màu phẳng và chuyển sắc.\n- Tuần 3-4: Vẽ hoa lá, thực hành kỹ thuật tỉa chi tiết.\n- Tuần 5-6: Vẽ phong cảnh bầu trời, mặt nước đơn giản.",
                    HocPhi = 2500000,
                    SoLuongBuoi = 12,
                    HinhAnh = "mauNuoc1.jpg",
                    NgayTao = DateOnly.FromDateTime(DateTime.Now)
                },
                new KhoaHoc
                {
                    IdChuyenMon = 3,
                    TenKhoaHoc = "Màu nước: Chân dung & Minh họa",
                    MoTa = "Nâng cao kỹ năng màu nước để vẽ chân dung và minh họa sách/truyện. Yêu cầu học viên đã biết kỹ thuật màu nước cơ bản.",
                    MucTieu = "Tả được da người, ngũ quan bằng màu nước. Sử dụng mixed-media (kết hợp bút kim, màu chì) trong tranh minh họa.",
                    LoTrinh = "- Phần 1 (6 buổi): Nghiên cứu giải phẫu khuôn mặt, cách pha màu da (skin tone).\n- Phần 2 (6 buổi): Vẽ chân dung bán thân, xử lý tóc và trang phục.\n- Phần 3 (4 buổi): Sáng tác tranh minh họa theo phong cách Fantasy hoặc Fashion.",
                    HocPhi = 3800000,
                    SoLuongBuoi = 16,
                    HinhAnh = "mauNuoc2.jpg",
                    NgayTao = DateOnly.FromDateTime(DateTime.Now)
                },

                // ============================================================
                // 4. MÀU ACRYLIC
                // ============================================================
                new KhoaHoc
                {
                    IdChuyenMon = 4,
                    TenKhoaHoc = "Acrylic: Vẽ tranh Decor ứng dụng",
                    MoTa = "Acrylic là chất liệu khô nhanh, bền màu và dễ sửa chữa. Khóa học này tập trung vào việc vẽ tranh trang trí nội thất.",
                    MucTieu = "Tự tay vẽ được tranh treo tường, vẽ lên vải (tote bag, áo) hoặc vẽ lên gỗ/đá.",
                    LoTrinh = "- Giai đoạn 1: Kỹ thuật pha màu Acrylic, cách đi cọ tạo texture.\n- Giai đoạn 2: Chép tranh phong cảnh hiện đại, tranh trừu tượng.\n- Giai đoạn 3: Thực hành vẽ trên chất liệu khác (vải canvas, gỗ) để làm sản phẩm ứng dụng.",
                    HocPhi = 2200000,
                    SoLuongBuoi = 12,
                    HinhAnh = "mauAcrylic1.jpg",
                    NgayTao = DateOnly.FromDateTime(DateTime.Now)
                },
                
                // ============================================================
                // 5. HÌNH HỌA (SKETCHING)
                // ============================================================
                new KhoaHoc
                {
                    IdChuyenMon = 5,
                    TenKhoaHoc = "Hình họa chì: Nền tảng vạn vật",
                    MoTa = "Khóa học quan trọng nhất cho bất kỳ ai muốn theo đuổi mỹ thuật chuyên nghiệp. Tập trung vào dựng hình và đánh bóng.",
                    MucTieu = "Rèn luyện mắt quan sát tỉ lệ chính xác. Hiểu về cấu trúc khối, ánh sáng và bóng đổ.",
                    LoTrinh = "- Level 1 (Buổi 1-5): Dựng các khối cơ bản (vuông, tròn, chóp). Tập đánh bóng tạo khối.\n- Level 2 (Buổi 6-10): Vẽ tĩnh vật tổ hợp (lọ hoa, quả, vải).\n- Level 3 (Buổi 11-16): Dựng hình đầu tượng phạt mảng (cơ sở để vẽ chân dung).",
                    HocPhi = 2800000,
                    SoLuongBuoi = 16,
                    HinhAnh = "hinhHoa-chanDung1.jpg",
                    NgayTao = DateOnly.FromDateTime(DateTime.Now)
                },
                new KhoaHoc
                {
                    IdChuyenMon = 5,
                    TenKhoaHoc = "Hình họa chì: Chân dung truyền thần",
                    MoTa = "Bước tiếp theo sau khóa hình họa cơ bản. Tập trung sâu vào ngũ quan và cảm xúc con người.",
                    MucTieu = "Vẽ được chân dung người thật sống động. Tả kỹ các chất liệu tóc, da, mắt.",
                    LoTrinh = "- Phần 1: Chi tiết ngũ quan (Mắt, Mũi, Miệng, Tai) ở các góc độ.\n- Phần 2: Dựng hình chân dung nam/nữ, người già/trẻ em.\n- Phần 3: Hoàn thiện chân dung tả thực với kỹ thuật đánh bóng chì than.",
                    HocPhi = 4200000,
                    SoLuongBuoi = 20,
                    HinhAnh = "hinhHoa-chanDung2.jpg",
                    NgayTao = DateOnly.FromDateTime(DateTime.Now)
                },

                // ============================================================
                // 6. LUYỆN THI KHỐI H (KIẾN TRÚC/MỸ THUẬT)
                // ============================================================
                new KhoaHoc
                {
                    IdChuyenMon = 6,
                    TenKhoaHoc = "Luyện thi Đại học Khối H - Dài hạn",
                    MoTa = "Chương trình chuẩn bị toàn diện cho kỳ thi đại học (Mỹ thuật CN, Kiến trúc...). Đi từ con số 0 đến khi thi. Khóa học nặng về kỹ thuật và tư duy bố cục.",
                    MucTieu = "Đạt điểm cao môn Hình họa (Người) và Trang trí màu. Nắm vững các dạng đề thi các năm.",
                    LoTrinh = "- Học kỳ 1: Hình họa cơ bản (Khối -> Tượng) và Nguyên lý màu sắc cơ bản.\n- Học kỳ 2: Hình họa nâng cao (Tượng chân dung) và Trang trí màu (Hàng lối, Đăng đối).\n- Học kỳ 3: Luyện đề thi thử, sửa lỗi sai, rèn tốc độ làm bài.",
                    HocPhi = 8000000, // Khóa dài hạn, kiến thức nặng
                    SoLuongBuoi = 40,
                    HinhAnh = "khoiH1.jpg",
                    NgayTao = DateOnly.FromDateTime(DateTime.Now)
                },
                new KhoaHoc
                {
                    IdChuyenMon = 6,
                    TenKhoaHoc = "Luyện thi Khối H - Giải đề cấp tốc",
                    MoTa = "Dành cho các bạn đã có nền tảng, cần tổng ôn và luyện kỹ năng phòng thi trong 2 tháng cuối trước kỳ thi.",
                    MucTieu = "Tối ưu hóa điểm số. Khắc phục các lỗi sai thường gặp khi áp lực thời gian. Biết 'mẹo' làm bài thi hiệu quả.",
                    LoTrinh = "- Tuần 1-4: Giải bộ đề hình họa và trang trí màu các trường Top (MTCN, Kiến Trúc HN/HCM).\n- Tuần 5-8: Thi thử dưới áp lực thời gian thực (4h/bài). Chấm chữa bài 1:1 chi tiết.",
                    HocPhi = 5500000, // Cấp tốc, cường độ cao
                    SoLuongBuoi = 24, // 3 buổi/tuần x 8 tuần
                    HinhAnh = "khoiH2.jpg",
                    NgayTao = DateOnly.FromDateTime(DateTime.Now)
                }
            };

            await dbContext.KhoaHocs.AddRangeAsync(khoaHocs);
            await dbContext.SaveChangesAsync();
        }

        if (dbContext.GiaoViens.Any() == false)
        {
            var teachers = new List<GiaoVien>
            {
                new GiaoVien { TenGv ="Phan Tấn trung", TrangThai=true,NgaySinh= DateOnly.FromDateTime(new DateTime(1990,5,1)),Sdt="0909123456",SoNamKinhNghiem=3,Avatar="PhanTanTrung.jpg" },
                new GiaoVien { TenGv ="Lê Thị Hồng", TrangThai=true,NgaySinh= DateOnly.FromDateTime(new DateTime(1985,3,15)),Sdt="0909234567",SoNamKinhNghiem=5,Avatar="LeThiHong.png" },
                new GiaoVien { TenGv ="Trần Văn An", TrangThai=true,NgaySinh= DateOnly.FromDateTime(new DateTime(1992,7,20)),Sdt="0909345678",SoNamKinhNghiem=4,Avatar="TranVanAn.jpg" },
                new GiaoVien { TenGv ="Nguyễn Thị Bích", TrangThai=true,NgaySinh= DateOnly.FromDateTime(new DateTime(1995,8,25)),Sdt="0909456789",SoNamKinhNghiem=2,Avatar="NguyenThiBich.jpg" }
            };
            dbContext.GiaoViens.AddRange(teachers);
            dbContext.SaveChanges();
        }

        if (dbContext.LopHocs.Any() == false)
        {
            var lopHocs = new List<LopHoc>
            {
                 new LopHoc{TenLopHoc="Lớp thiếu nhi căn bản thầy Trung",IdPhong=1,IdKhoaHoc=1,NgayKhaiGiang= DateOnly.FromDateTime(DateTime.Now.AddDays(7)),SoLuongBuoi=8,SoBuoiTrenTuan="7",NgayTao= DateOnly.FromDateTime(DateTime.Now),SoLuongHv=0,SoLuongToiDa=10,SoLuongToiThieu=5,ThoiGianBatDau=TimeOnly.FromTimeSpan(new TimeSpan(15,0,0)),ThoiGianKetThuc=TimeOnly.FromTimeSpan(new TimeSpan(17,0,0)),TrangThai=DungChung.trangThaiLopHoc_DangMo},
            };
            dbContext.LopHocs.AddRange(lopHocs);
            dbContext.SaveChanges();

            var lop = dbContext.LopHocs.FirstOrDefault(lh => lh.TenLopHoc == "Lớp thiếu nhi căn bản thầy Trung");
            var gv = dbContext.GiaoViens.FirstOrDefault(gv => gv.TenGv == "Phan Tấn trung");
            if (lop != null && gv != null)
            {
                var giaoVienDayLop = new GiaoVienDdayLop
                {
                    IdGiaoVien = gv.GiaoVienId,
                    IdLopHoc = lop.IdLopHoc
                };
                dbContext.GiaoVienDdayLops.Add(giaoVienDayLop);
                dbContext.SaveChanges();
            }
        }

    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while migrating the database.");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.Use(async (context, next) =>
// {
//     context.Response.Headers["Cross-Origin-Opener-Policy"] = "unsafe-none";
//     context.Response.Headers["Cross-Origin-Embedder-Policy"] = "unsafe-none";
//     await next();
// });
app.UseStaticFiles();





app.UseCors("CorsPolicy");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard();

//Lên lịch cho hangfire

RecurringJob.AddOrUpdate<HangFire_Services>(
    "CheckAndUpdateClassStatus",
    service => service.CheckAndUpdateClassStatus(),
    Cron.Daily(1)
);

app.MapControllers();
// var summaries = new[]
// {
//     "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
// };

// app.MapGet("/weatherforecast", () =>
// {
//     var forecast =  Enumerable.Range(1, 5).Select(index =>
//         new WeatherForecast
//         (
//             DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//             Random.Shared.Next(-20, 55),
//             summaries[Random.Shared.Next(summaries.Length)]
//         ))
//         .ToArray();
//     return forecast;
// })
// .WithName("GetWeatherForecast")
// .WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
