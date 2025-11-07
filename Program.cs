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



var app = builder.Build();

app.UseForwardedHeaders();

try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
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
            dbContext.Quyens.AddRange(adminRole, nhanVienKhoRole);
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
        }
        if (dbContext.ChuyenMons.Any() == false)
        {
            var chuyenMons = new List<ChuyenMon>
            {
                new ChuyenMon { TenChuyenMon = "Mỹ thuật thiếu nhi", MoTa = "Khi khả năng ngôn ngữ phát triển chưa hoàn thiện, hội họa là phương tiện để diễn đạt hiệu quả. Nét vẽ nguệch ngoạc, hồn nhiên, bình dị nhưng rất cần thiết trong quá trình hình thành khả năng cảm thụ cái đẹp và tư duy sáng tạo của trẻ. Đó là cảm xúc, tình cảm, ước mơ khám phá thế giới  xung quanh mà trẻ thể hiện trên trang giấy", HinhAnh = "MyThuatThieuNhi.jpg" },
                new ChuyenMon { TenChuyenMon = "Sơn dầu", MoTa = "Tranh sơn dầu được rất nhiều người yêu mến hội họa yêu thích với phong cách mềm mại, bóng bẩy đầy ma lực và sự lôi cuốn.", HinhAnh = "SonDau.jpg" },
                new ChuyenMon { TenChuyenMon = "Màu nước", MoTa = "Màu nước thường được dùng với bút pháp rộng rãi, xây dựng bố cục bằng những mảng lớn nhưng lại sâu sắc, mượt mà về sắc điệu gây một cảm giác rung động khó tả. Đây là loại chất liệu khó sử dụng nên phụ thuộc rất nhiều vào tài năng sáng tạo và khí chất của người vẽ .“Cứ vẽ đi vẽ lại một trăm lần thì bức tranh sẽ đơn giản đi”.", HinhAnh = "MauNuoc.jpg" },
                new ChuyenMon { TenChuyenMon = "Màu Acrylic", MoTa = "Màu Acrylic là loại màu có độ phủ cao, độ bám dính cực tốt, màu sắc rất đa dạng, không độc hại, không phai màu, không thấm nước. Với đặc tính đó, kết hợp với sự đơn gian cùng với các kỹ thuật đa dạng sẽ giúp cho người học đắm chìm trong thể loại tranh Arcylic với thời gian ngắn.", HinhAnh = "MauAcrylic.jpg" },

                new ChuyenMon {TenChuyenMon="Hình họa - Vẽ chân dung",MoTa="Hình họa là môn học cơ bản nhằm rèn luyện nhận thức thẩm mỹ và kỹ năng thể hiện hình khối không gian. Khóa học giúp học viên nắm vững kiến thức, phát triển kỹ năng hội họa, tự tin thể hiện các ý tưởng và các bài thi về hình họa.",HinhAnh="ChanDung.jpg"},
                new ChuyenMon {TenChuyenMon="Luyện thi khôi H",MoTa="KSL khuyến khích các bạn nên bắt đầu học luyện sớm ngay khi nhận ra được đam mê của mình, để bạn có đủ thời gian học từ CĂN BẢN lên NÂNG CAO, điều đó sẽ giúp bạn có  nền tảng vững chắc và tự tin cho kỳ thi tuyển sinh của mình.",HinhAnh="LuyenThiKhoiH.jpg"},
            };

            dbContext.ChuyenMons.AddRange(chuyenMons);
            dbContext.SaveChanges();
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
                // === Mỹ thuật thiếu nhi ===
                new KhoaHoc
                {
                    IdChuyenMon = 1,
                    TenKhoaHoc = "Mỹ thuật thiếu nhi Căn Bản",
                    MoTa = "Khóa học xây dựng nền tảng mỹ thuật cho trẻ qua các hoạt động vui chơi với màu sắc, hình khối và chất liệu đa dạng. Bé được tự do khám phá và thể hiện thế giới quan sinh động của mình.",
                    MucTieu = "Kích thích khả năng quan sát, trí tưởng tượng và sự tự tin của trẻ. Giúp trẻ làm quen với các dụng cụ vẽ cơ bản và phát triển vận động tinh qua các hoạt động tạo hình.",
                    HocPhi = 2200000,
                    SoLuongBuoi = 36,

                    HinhAnh = "thieuNhi1.jpg",
                    NgayTao = DateOnly.FromDateTime(DateTime.Now)
                },
                new KhoaHoc
                {
                    IdChuyenMon = 1,
                    TenKhoaHoc = "Mỹ thuật thiếu nhi Nâng Cao",
                    MoTa = "Chương trình học sâu hơn về bố cục, phối màu và kỹ thuật vẽ theo chủ đề. Trẻ được hướng dẫn để kể những câu chuyện sáng tạo thông qua tác phẩm của mình một cách bài bản hơn.",
                    MucTieu = "Phát triển tư duy kể chuyện bằng hình ảnh. Nâng cao kỹ năng sử dụng màu sắc và tạo hình nhân vật, không gian. Xây dựng sự tự tin và phong cách cá nhân cho trẻ.",
                    HocPhi = 3500000,
                    SoLuongBuoi = 36,

                    HinhAnh = "thieuNhi2.jpg",
                    NgayTao = DateOnly.FromDateTime(DateTime.Now)
                },

                // === Sơn dầu ===
                new KhoaHoc
                {
                    IdChuyenMon = 2,
                    TenKhoaHoc = "Sơn dầu Căn Bản",
                    MoTa = "Khóa học nhập môn về chất liệu sơn dầu, bao gồm cách pha màu, sử dụng các loại bút vẽ, và các kỹ thuật vẽ cơ bản như đi nét, đánh bóng, tạo khối.",
                    MucTieu = "Hiểu rõ đặc tính của sơn dầu. Nắm vững kỹ thuật pha màu cơ bản và cách thể hiện ánh sáng, bóng đổ. Hoàn thiện một bức tranh tĩnh vật đơn giản.",
                    HocPhi = 3000000,
                    SoLuongBuoi = 36,

                    HinhAnh = "sonDau1.jpg",
                    NgayTao = DateOnly.FromDateTime(DateTime.Now)
                },
                new KhoaHoc
                {
                    IdChuyenMon = 2,
                    TenKhoaHoc = "Sơn dầu Nâng Cao",
                    MoTa = "Đi sâu vào các kỹ thuật phức tạp như vẽ nhiều lớp (glazing), vẽ dày (impasto) và nghiên cứu chuyên sâu về vẽ chân dung, phong cảnh bằng sơn dầu.",
                    MucTieu = "Làm chủ các kỹ thuật vẽ sơn dầu phức tạp. Định hình phong cách nghệ thuật cá nhân. Tự tin sáng tác các tác phẩm có chiều sâu và cảm xúc.",
                    HocPhi = 5000000,
                    SoLuongBuoi = 36,

                    HinhAnh = "sonDau2.jpg",
                    NgayTao = DateOnly.FromDateTime(DateTime.Now)
                },

                // === Màu nước ===
                new KhoaHoc
                {
                    IdChuyenMon = 3,
                    TenKhoaHoc = "Màu nước Căn Bản",
                    MoTa = "Khóa học giới thiệu về sự kỳ diệu của màu nước, từ cách kiểm soát lượng nước, loang màu, đến các kỹ thuật cơ bản như vẽ ướt trên ướt và ướt trên khô.",
                    MucTieu = "Hiểu được tính chất trong trẻo và ngẫu hứng của màu nước. Nắm vững các kỹ thuật cơ bản để tạo hiệu ứng. Thực hành vẽ các chủ đề đơn giản như hoa lá, đồ vật.",
                    HocPhi = 2800000,
                    SoLuongBuoi = 36,

                    HinhAnh = "mauNuoc1.jpg",
                    NgayTao = DateOnly.FromDateTime(DateTime.Now)
                },
                new KhoaHoc
                {
                    IdChuyenMon = 3,
                    TenKhoaHoc = "Màu nước Nâng Cao",
                    MoTa = "Nghiên cứu sâu về các kỹ thuật khó như masking, cạo màu, sử dụng muối và cồn để tạo hiệu ứng đặc biệt. Tập trung vào vẽ phong cảnh và chân dung màu nước.",
                    MucTieu = "Thành thạo việc kiểm soát nước và màu sắc. Sáng tạo với các hiệu ứng đặc biệt. Thể hiện được chiều sâu và không khí trong tranh phong cảnh, chân dung.",
                    HocPhi = 4500000,
                    SoLuongBuoi = 36,

                    HinhAnh = "mauNuoc2.jpg",
                    NgayTao = DateOnly.FromDateTime(DateTime.Now)
                },
                
                // === Màu Acrylic ===
                new KhoaHoc
                {
                    IdChuyenMon = 4,
                    TenKhoaHoc = "Màu Acrylic Căn Bản",
                    MoTa = "Khám phá sự linh hoạt của màu Acrylic, một chất liệu khô nhanh và đa dụng. Học viên sẽ học cách pha màu, chồng lớp và các kỹ thuật cơ bản để bắt đầu sáng tác.",
                    MucTieu = "Nắm rõ ưu và nhược điểm của màu Acrylic. Làm chủ kỹ thuật pha màu và đi nét. Hoàn thành được một tác phẩm tranh trừu tượng hoặc phong cảnh đơn giản.",
                    HocPhi = 2700000,
                    SoLuongBuoi = 36,

                    HinhAnh = "mauAcrylic1.jpg",
                    NgayTao = DateOnly.FromDateTime(DateTime.Now)
                },
                new KhoaHoc
                {
                    IdChuyenMon = 4,
                    TenKhoaHoc = "Màu Acrylic Nâng Cao",
                    MoTa = "Khóa học tập trung vào việc ứng dụng Acrylic trong các phong cách nghệ thuật khác nhau, từ tả thực đến pop-art. Tìm hiểu các chất phụ gia (medium) để thay đổi đặc tính của màu.",
                    MucTieu = "Sử dụng thành thạo các loại medium. Phát triển khả năng sáng tác trên nhiều chất liệu nền khác nhau. Tự tin thể hiện ý tưởng nghệ thuật phức tạp bằng Acrylic.",
                    HocPhi = 4200000,
                    SoLuongBuoi = 36,

                    HinhAnh = "mauAcrylic2.jpg",
                    NgayTao = DateOnly.FromDateTime(DateTime.Now)
                },
                
                // === Hình họa - Vẽ chân dung ===
                new KhoaHoc
                {
                    IdChuyenMon = 5,
                    TenKhoaHoc = "Hình họa - Chân dung Căn Bản",
                    MoTa = "Xây dựng nền tảng vững chắc về dựng hình, tỷ lệ và giải phẫu khuôn mặt. Học viên sẽ được luyện tập cách quan sát và dựng hình khối cơ bản của đầu, mắt, mũi, miệng.",
                    MucTieu = "Nắm vững tỷ lệ vàng của khuôn mặt. Dựng hình chính xác các chi tiết ngũ quan. Hiểu về cấu trúc khối và cách đi nét để tạo cảm giác không gian.",
                    HocPhi = 3200000,
                    SoLuongBuoi = 36,

                    HinhAnh = "hinhHoa-chanDung1.jpg",
                    NgayTao = DateOnly.FromDateTime(DateTime.Now)
                },
                new KhoaHoc
                {
                    IdChuyenMon = 5,
                    TenKhoaHoc = "Hình họa - Chân dung Nâng Cao",
                    MoTa = "Tập trung vào việc lột tả thần thái và cảm xúc của nhân vật. Nghiên cứu sâu về chất liệu (da, tóc, vải) và cách diễn tả chúng bằng chì, than.",
                    MucTieu = "Nâng cao khả năng diễn tả cảm xúc nhân vật. Làm chủ kỹ thuật tả chất liệu. Hoàn thiện các bức chân dung có hồn và chiều sâu.",
                    HocPhi = 5500000,
                    SoLuongBuoi = 36,

                    HinhAnh = "hinhHoa-chanDung2.jpg",
                    NgayTao = DateOnly.FromDateTime(DateTime.Now)
                },

                // === Luyện thi khối H ===
                new KhoaHoc
                {
                    IdChuyenMon = 6,
                    TenKhoaHoc = "Luyện thi khối H - Nền tảng",
                    MoTa = "Chương trình được thiết kế để xây dựng nền tảng vững chắc về hình họa (đầu tượng, tĩnh vật) và trang trí màu, bám sát cấu trúc đề thi của các trường đại học.",
                    MucTieu = "Nắm vững kiến thức căn bản về dựng hình, sắc độ, bố cục. Hiểu nguyên lý màu sắc và cách điệu. Làm quen với áp lực thời gian và không khí phòng thi.",
                    HocPhi = 3800000,
                    SoLuongBuoi = 36,

                    HinhAnh = "khoiH1.jpg",
                    NgayTao = DateOnly.FromDateTime(DateTime.Now)
                },
                new KhoaHoc
                {
                    IdChuyenMon = 6,
                    TenKhoaHoc = "Luyện thi khối H - Cấp tốc",
                    MoTa = "Khóa học tập trung vào việc giải đề, phân tích các lỗi sai thường gặp và rèn luyện kỹ năng, chiến thuật làm bài thi để tối ưu hóa điểm số trong thời gian ngắn.",
                    MucTieu = "Thành thạo các dạng đề thi. Tối ưu hóa tốc độ và hiệu quả làm bài. Nâng cao kỹ năng phân tích đề và xây dựng bố cục. Tự tin bước vào kỳ thi quan trọng.",
                    HocPhi = 6000000,
                    SoLuongBuoi = 36,

                    HinhAnh = "khoiH2.jpg",
                    NgayTao = DateOnly.FromDateTime(DateTime.Now)
                },
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
                 new LopHoc{TenLopHoc="Lớp thiếu nhi căn bản thầy Trung",IdPhong=1,IdKhoaHoc=1,NgayKhaiGiang= DateOnly.FromDateTime(DateTime.Now.AddDays(7)),SoLuongBuoi=36,SoBuoiTrenTuan="1,3,5",NgayTao= DateOnly.FromDateTime(DateTime.Now),SoLuongHv=0,SoLuongToiDa=10,SoLuongToiThieu=5,ThoiGianBatDau=TimeOnly.FromTimeSpan(new TimeSpan(15,0,0)),ThoiGianKetThuc=TimeOnly.FromTimeSpan(new TimeSpan(17,0,0)),TrangThai=DungChung.trangThaiLopHoc_DangMo},
            };
            dbContext.LopHocs.AddRange(lopHocs);
            var giaoVienDayLop= new GiaoVienDdayLop
            {
                IdGiaoVien = dbContext.GiaoViens.FirstOrDefault(gv => gv.TenGv == "Phan Tấn trung").GiaoVienId,
                IdLopHoc = dbContext.LopHocs.FirstOrDefault(lh => lh.TenLopHoc == "Lớp thiếu nhi căn bản thầy Trung").IdLopHoc
            };
            dbContext.GiaoVienDdayLops.Add(giaoVienDayLop);
            dbContext.SaveChanges();
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
