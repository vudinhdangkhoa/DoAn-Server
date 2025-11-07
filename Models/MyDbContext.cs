using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace server.Models;

public partial class MyDbContext : DbContext
{
    public MyDbContext()
    {
    }

    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CacHocCuKhuyenMai> CacHocCuKhuyenMais { get; set; }

    public virtual DbSet<CacKhoaHocKhuyenMai> CacKhoaHocKhuyenMais { get; set; }

    public virtual DbSet<ChiTietHoaDonHocCu> ChiTietHoaDonHocCus { get; set; }

    public virtual DbSet<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; }

    public virtual DbSet<ChuyenMon> ChuyenMons { get; set; }

    public virtual DbSet<GiaoVien> GiaoViens { get; set; }

    public virtual DbSet<GiaoVienDdayLop> GiaoVienDdayLops { get; set; }

    public virtual DbSet<HoaDonHocCu> HoaDonHocCus { get; set; }

    public virtual DbSet<HoaDonKhoaHoc> HoaDonKhoaHocs { get; set; }

    public virtual DbSet<HocCu> HocCus { get; set; }

    public virtual DbSet<HocCuThuocLop> HocCuThuocLops { get; set; }

    public virtual DbSet<HocVien> HocViens { get; set; }

    public virtual DbSet<KhoaHoc> KhoaHocs { get; set; }

    public virtual DbSet<KhuyenMai> KhuyenMais { get; set; }

    public virtual DbSet<LichHoc> LichHocs { get; set; }

    public virtual DbSet<LoaiHocCu> LoaiHocCus { get; set; }

    public virtual DbSet<LopHoc> LopHocs { get; set; }

    public virtual DbSet<NhaCungCap> NhaCungCaps { get; set; }

    public virtual DbSet<NhanVien> NhanViens { get; set; }

    public virtual DbSet<PhanHoi> PhanHois { get; set; }

    public virtual DbSet<PhieuNhapHang> PhieuNhapHangs { get; set; }

    public virtual DbSet<PhongHoc> PhongHocs { get; set; }

    public virtual DbSet<PhuHuynh> PhuHuynhs { get; set; }

    public virtual DbSet<Quyen> Quyens { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=KUPHA;Database=QL_TrungTamMyThuat;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CacHocCuKhuyenMai>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__cacHocCu__3213E83FE8259E2F");

            entity.ToTable("cacHocCuKhuyenMai");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdHocCu).HasColumnName("idHocCu");
            entity.Property(e => e.IdKhuyenMai).HasColumnName("idKhuyenMai");
            entity.Property(e => e.NgayBatDau)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("ngayBatDau");
            entity.Property(e => e.NgayKetThuc).HasColumnName("ngayKetThuc");
            entity.Property(e => e.SoLuong).HasColumnName("soLuong");

            entity.HasOne(d => d.IdHocCuNavigation).WithMany(p => p.CacHocCuKhuyenMais)
                .HasForeignKey(d => d.IdHocCu)
                .HasConstraintName("FK__cacHocCuK__idHoc__07C12930");

            entity.HasOne(d => d.IdKhuyenMaiNavigation).WithMany(p => p.CacHocCuKhuyenMais)
                .HasForeignKey(d => d.IdKhuyenMai)
                .HasConstraintName("FK__cacHocCuK__idKhu__08B54D69");
        });

        modelBuilder.Entity<CacKhoaHocKhuyenMai>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__cacKhoaH__3213E83FE73C4B92");

            entity.ToTable("cacKhoaHocKhuyenMai");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdKhoaHoc).HasColumnName("idKhoaHoc");
            entity.Property(e => e.IdKhuyenMai).HasColumnName("idKhuyenMai");
            entity.Property(e => e.NgayBatDau)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("ngayBatDau");
            entity.Property(e => e.NgayKetThuc).HasColumnName("ngayKetThuc");
            entity.Property(e => e.SoLuong).HasColumnName("soLuong");

            entity.HasOne(d => d.IdKhoaHocNavigation).WithMany(p => p.CacKhoaHocKhuyenMais)
                .HasForeignKey(d => d.IdKhoaHoc)
                .HasConstraintName("FK__cacKhoaHo__idKho__03F0984C");

            entity.HasOne(d => d.IdKhuyenMaiNavigation).WithMany(p => p.CacKhoaHocKhuyenMais)
                .HasForeignKey(d => d.IdKhuyenMai)
                .HasConstraintName("FK__cacKhoaHo__idKhu__02FC7413");
        });

        modelBuilder.Entity<ChiTietHoaDonHocCu>(entity =>
        {
            entity.HasKey(e => new { e.IdHoaDonHocCu, e.IdHocCu }).HasName("PK__chiTietH__3A55BF5BBA5E3AFF");

            entity.ToTable("chiTietHoaDonHocCu");

            entity.Property(e => e.IdHoaDonHocCu).HasColumnName("idHoaDonHocCu");
            entity.Property(e => e.IdHocCu).HasColumnName("idHocCu");
            entity.Property(e => e.SoLuong).HasColumnName("soLuong");

            entity.HasOne(d => d.IdHoaDonHocCuNavigation).WithMany(p => p.ChiTietHoaDonHocCus)
                .HasForeignKey(d => d.IdHoaDonHocCu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__chiTietHo__idHoa__7C4F7684");

            entity.HasOne(d => d.IdHocCuNavigation).WithMany(p => p.ChiTietHoaDonHocCus)
                .HasForeignKey(d => d.IdHocCu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__chiTietHo__idHoc__7D439ABD");
        });

        modelBuilder.Entity<ChiTietPhieuNhap>(entity =>
        {
            entity.HasKey(e => new { e.IdPhieuNhapHang, e.IdHocCu }).HasName("PK__chiTietP__A820FB6A96351030");

            entity.ToTable("chiTietPhieuNhap");

            entity.Property(e => e.IdPhieuNhapHang).HasColumnName("idPhieuNhapHang");
            entity.Property(e => e.IdHocCu).HasColumnName("idHocCu");
            entity.Property(e => e.Gia).HasColumnName("gia");
            entity.Property(e => e.SoLuong).HasColumnName("soLuong");

            entity.HasOne(d => d.IdHocCuNavigation).WithMany(p => p.ChiTietPhieuNhaps)
                .HasForeignKey(d => d.IdHocCu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__chiTietPh__idHoc__70DDC3D8");

            entity.HasOne(d => d.IdPhieuNhapHangNavigation).WithMany(p => p.ChiTietPhieuNhaps)
                .HasForeignKey(d => d.IdPhieuNhapHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__chiTietPh__idPhi__71D1E811");
        });

        modelBuilder.Entity<ChuyenMon>(entity =>
        {
            entity.HasKey(e => e.IdChuyenMon).HasName("PK__chuyenMo__EF3A9E97532B18D7");

            entity.ToTable("chuyenMon");

            entity.Property(e => e.IdChuyenMon).HasColumnName("idChuyenMon");
            entity.Property(e => e.HinhAnh)
                .IsUnicode(false)
                .HasColumnName("hinhAnh");
            entity.Property(e => e.MoTa).HasColumnName("moTa");
            entity.Property(e => e.TenChuyenMon)
                .HasMaxLength(100)
                .HasColumnName("tenChuyenMon");
        });

        modelBuilder.Entity<GiaoVien>(entity =>
        {
            entity.HasKey(e => e.GiaoVienId).HasName("PK__giaoVien__3E3E99AB81D85C1F");

            entity.ToTable("giaoVien");

            entity.Property(e => e.GiaoVienId).HasColumnName("giaoVienID");
            entity.Property(e => e.Avatar)
                .IsUnicode(false)
                .HasColumnName("avatar");
            entity.Property(e => e.NgaySinh).HasColumnName("ngaySinh");
            entity.Property(e => e.Sdt)
                .HasMaxLength(12)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("sdt");
            entity.Property(e => e.SoNamKinhNghiem).HasColumnName("soNamKinhNghiem");
            entity.Property(e => e.TenGv)
                .HasMaxLength(100)
                .HasColumnName("tenGV");
            entity.Property(e => e.TrangThai)
                .HasDefaultValue(true)
                .HasColumnName("trangThai");
        });

        modelBuilder.Entity<GiaoVienDdayLop>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__giaoVien__3213E83FE122CF84");

            entity.ToTable("giaoVienDdayLop");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdGiaoVien).HasColumnName("idGiaoVien");
            entity.Property(e => e.IdLopHoc).HasColumnName("idLopHoc");

            entity.HasOne(d => d.IdGiaoVienNavigation).WithMany(p => p.GiaoVienDdayLops)
                .HasForeignKey(d => d.IdGiaoVien)
                .HasConstraintName("FK__giaoVienD__idGia__59063A47");

            entity.HasOne(d => d.IdLopHocNavigation).WithMany(p => p.GiaoVienDdayLops)
                .HasForeignKey(d => d.IdLopHoc)
                .HasConstraintName("FK__giaoVienD__idLop__5812160E");
        });

        modelBuilder.Entity<HoaDonHocCu>(entity =>
        {
            entity.HasKey(e => e.IdHoaDonHocCu).HasName("PK__hoaDonHo__0E95D3FF65DF938F");

            entity.ToTable("hoaDonHocCu");

            entity.Property(e => e.IdHoaDonHocCu).HasColumnName("idHoaDonHocCu");
            entity.Property(e => e.GiamGia).HasColumnName("giamGia");
            entity.Property(e => e.IdKhachHang).HasColumnName("idKhachHang");
            entity.Property(e => e.IdNhanVien).HasColumnName("idNhanVien");
            entity.Property(e => e.Sdt)
                .HasMaxLength(12)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("sdt");
            entity.Property(e => e.TenKh)
                .HasMaxLength(100)
                .HasColumnName("tenKH");
            entity.Property(e => e.TongTien).HasColumnName("tongTien");

            entity.HasOne(d => d.IdKhachHangNavigation).WithMany(p => p.HoaDonHocCus)
                .HasForeignKey(d => d.IdKhachHang)
                .HasConstraintName("FK__hoaDonHoc__idKha__797309D9");

            entity.HasOne(d => d.IdNhanVienNavigation).WithMany(p => p.HoaDonHocCus)
                .HasForeignKey(d => d.IdNhanVien)
                .HasConstraintName("FK__hoaDonHoc__idNha__787EE5A0");
        });

        modelBuilder.Entity<HoaDonKhoaHoc>(entity =>
        {
            entity.HasKey(e => e.IdHoaDon).HasName("PK__hoaDonKh__B060C52C76DDDD5D");

            entity.ToTable("hoaDonKhoaHoc");

            entity.Property(e => e.IdHoaDon).HasColumnName("idHoaDon");
            entity.Property(e => e.GiamGia).HasColumnName("giamGia");
            entity.Property(e => e.HocVienId).HasColumnName("hocVienID");
            entity.Property(e => e.IdKhoaHoc).HasColumnName("idKhoaHoc");
            entity.Property(e => e.IdLopHoc).HasColumnName("idLopHoc");
            entity.Property(e => e.Ngaytao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("ngaytao");
            entity.Property(e => e.TongTien).HasColumnName("tongTien");
            entity.Property(e => e.TrangThai)
                .HasDefaultValue(false)
                .HasColumnName("trangThai");

            entity.HasOne(d => d.HocVien).WithMany(p => p.HoaDonKhoaHocs)
                .HasForeignKey(d => d.HocVienId)
                .HasConstraintName("FK__hoaDonKho__hocVi__0E6E26BF");

            entity.HasOne(d => d.IdKhoaHocNavigation).WithMany(p => p.HoaDonKhoaHocs)
                .HasForeignKey(d => d.IdKhoaHoc)
                .HasConstraintName("FK__hoaDonKho__idKho__0D7A0286");

            entity.HasOne(d => d.IdLopHocNavigation).WithMany(p => p.HoaDonKhoaHocs)
                .HasForeignKey(d => d.IdLopHoc)
                .HasConstraintName("FK__hoaDonKho__idLop__0C85DE4D");
        });

        modelBuilder.Entity<HocCu>(entity =>
        {
            entity.HasKey(e => e.IdHocCu).HasName("PK__hocCu__4C06CA4244A8D1D5");

            entity.ToTable("hocCu");

            entity.Property(e => e.IdHocCu).HasColumnName("idHocCu");
            entity.Property(e => e.DonViTinh)
                .HasMaxLength(100)
                .HasColumnName("donViTinh");
            entity.Property(e => e.GiaBan).HasColumnName("giaBan");
            entity.Property(e => e.IdLoaiHocCu).HasColumnName("idLoaiHocCu");
            entity.Property(e => e.SoLuong).HasColumnName("soLuong");
            entity.Property(e => e.TenHocCu)
                .HasMaxLength(100)
                .HasColumnName("tenHocCu");

            entity.HasOne(d => d.IdLoaiHocCuNavigation).WithMany(p => p.HocCus)
                .HasForeignKey(d => d.IdLoaiHocCu)
                .HasConstraintName("FK__hocCu__idLoaiHoc__6754599E");
        });

        modelBuilder.Entity<HocCuThuocLop>(entity =>
        {
            entity.HasKey(e => new { e.IdHocCu, e.IdLopHoc }).HasName("PK__hocCuThu__F7B52377AD0688F3");

            entity.ToTable("hocCuThuocLop");

            entity.Property(e => e.IdHocCu).HasColumnName("idHocCu");
            entity.Property(e => e.IdLopHoc).HasColumnName("idLopHoc");
            entity.Property(e => e.SoLuong).HasColumnName("soLuong");

            entity.HasOne(d => d.IdHocCuNavigation).WithMany(p => p.HocCuThuocLops)
                .HasForeignKey(d => d.IdHocCu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__hocCuThuo__idHoc__74AE54BC");

            entity.HasOne(d => d.IdLopHocNavigation).WithMany(p => p.HocCuThuocLops)
                .HasForeignKey(d => d.IdLopHoc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__hocCuThuo__idLop__75A278F5");
        });

        modelBuilder.Entity<HocVien>(entity =>
        {
            entity.HasKey(e => e.IdHocVien).HasName("PK__HocVien__4B013DD57A968840");

            entity.ToTable("HocVien");

            entity.Property(e => e.IdHocVien).HasColumnName("idHocVien");
            entity.Property(e => e.Avartar)
                .IsUnicode(false)
                .HasColumnName("avartar");
            entity.Property(e => e.GioiTinh)
                .HasMaxLength(20)
                .HasColumnName("gioiTinh");
            entity.Property(e => e.IdPhuHuynh).HasColumnName("idPhuHuynh");
            entity.Property(e => e.NgaySinh).HasColumnName("ngaySinh");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("ngayTao");
            entity.Property(e => e.TenHv)
                .HasMaxLength(100)
                .HasColumnName("tenHV");

            entity.HasOne(d => d.IdPhuHuynhNavigation).WithMany(p => p.HocViens)
                .HasForeignKey(d => d.IdPhuHuynh)
                .HasConstraintName("FK__HocVien__idPhuHu__403A8C7D");
        });

        modelBuilder.Entity<KhoaHoc>(entity =>
        {
            entity.HasKey(e => e.IdKhoaHoc).HasName("PK__khoaHoc__BD521AA8202F3B74");

            entity.ToTable("khoaHoc");

            entity.Property(e => e.IdKhoaHoc).HasColumnName("idKhoaHoc");
            entity.Property(e => e.HinhAnh)
                .IsUnicode(false)
                .HasColumnName("hinhAnh");
            entity.Property(e => e.HocPhi).HasColumnName("hocPhi");
            entity.Property(e => e.IdChuyenMon).HasColumnName("idChuyenMon");
            entity.Property(e => e.MoTa).HasColumnName("moTa");
            entity.Property(e => e.MucTieu).HasColumnName("mucTieu");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("ngayTao");
            entity.Property(e => e.SoLuongBuoi).HasColumnName("soLuongBuoi");
            entity.Property(e => e.TenKhoaHoc)
                .HasMaxLength(200)
                .HasColumnName("tenKhoaHoc");

            entity.HasOne(d => d.IdChuyenMonNavigation).WithMany(p => p.KhoaHocs)
                .HasForeignKey(d => d.IdChuyenMon)
                .HasConstraintName("FK__khoaHoc__idChuye__4E88ABD4");
        });

        modelBuilder.Entity<KhuyenMai>(entity =>
        {
            entity.HasKey(e => e.IdKhuyenMai).HasName("PK__khuyenMa__637EEC7CC8F0A87B");

            entity.ToTable("khuyenMai");

            entity.Property(e => e.IdKhuyenMai).HasColumnName("idKhuyenMai");
            entity.Property(e => e.PhanTramKhuyenMai).HasColumnName("phanTramKhuyenMai");
            entity.Property(e => e.TenKhuyenMai)
                .HasMaxLength(200)
                .HasColumnName("tenKhuyenMai");
        });

        modelBuilder.Entity<LichHoc>(entity =>
        {
            entity.HasKey(e => e.IdLichHoc).HasName("PK__lichHoc__C82DCAB39388E61A");

            entity.ToTable("lichHoc");

            entity.Property(e => e.IdLichHoc).HasColumnName("idLichHoc");
            entity.Property(e => e.IdLopHoc).HasColumnName("idLopHoc");
            entity.Property(e => e.IdPhong).HasColumnName("idPhong");
            entity.Property(e => e.NgayHoc).HasColumnName("ngayHoc");
            entity.Property(e => e.ThoiGianBatDau).HasColumnName("thoiGianBatDau");
            entity.Property(e => e.ThoiGianKetThuc).HasColumnName("thoiGianKetThuc");
            entity.Property(e => e.TrangThai)
                .HasDefaultValue(true)
                .HasColumnName("trangThai");

            entity.HasOne(d => d.IdLopHocNavigation).WithMany(p => p.LichHocs)
                .HasForeignKey(d => d.IdLopHoc)
                .HasConstraintName("FK__lichHoc__idLopHo__628FA481");

            entity.HasOne(d => d.IdPhongNavigation).WithMany(p => p.LichHocs)
                .HasForeignKey(d => d.IdPhong)
                .HasConstraintName("FK__lichHoc__idPhong__619B8048");
        });

        modelBuilder.Entity<LoaiHocCu>(entity =>
        {
            entity.HasKey(e => e.IdLoaiHocCu).HasName("PK__loaiHocC__006732AB57B6A816");

            entity.ToTable("loaiHocCu");

            entity.Property(e => e.IdLoaiHocCu).HasColumnName("idLoaiHocCu");
            entity.Property(e => e.TenLoai)
                .HasMaxLength(200)
                .HasColumnName("tenLoai");
        });

        modelBuilder.Entity<LopHoc>(entity =>
        {
            entity.HasKey(e => e.IdLopHoc).HasName("PK__lopHoc__BB3E935A216BF780");

            entity.ToTable("lopHoc", tb => tb.HasTrigger("taoLichHoc"));

            entity.Property(e => e.IdLopHoc).HasColumnName("idLopHoc");
            entity.Property(e => e.IdKhoaHoc).HasColumnName("idKhoaHoc");
            entity.Property(e => e.IdPhong).HasColumnName("idPhong");
            entity.Property(e => e.NgayKhaiGiang).HasColumnName("ngayKhaiGiang");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("ngayTao");
            entity.Property(e => e.SoBuoiTrenTuan)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("soBuoiTrenTuan");
            entity.Property(e => e.SoLuongBuoi).HasColumnName("soLuongBuoi");
            entity.Property(e => e.SoLuongHv).HasColumnName("soLuongHV");
            entity.Property(e => e.SoLuongToiDa).HasColumnName("soLuongToiDa");
            entity.Property(e => e.SoLuongToiThieu).HasColumnName("soLuongToiThieu");
            entity.Property(e => e.TenLopHoc).HasColumnName("tenLopHoc");
            entity.Property(e => e.ThoiGianBatDau).HasColumnName("thoiGianBatDau");
            entity.Property(e => e.ThoiGianKetThuc).HasColumnName("thoiGianKetThuc");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasColumnName("trangThai");

            entity.HasOne(d => d.IdKhoaHocNavigation).WithMany(p => p.LopHocs)
                .HasForeignKey(d => d.IdKhoaHoc)
                .HasConstraintName("FK__lopHoc__idKhoaHo__5441852A");

            entity.HasOne(d => d.IdPhongNavigation).WithMany(p => p.LopHocs)
                .HasForeignKey(d => d.IdPhong)
                .HasConstraintName("FK__lopHoc__idPhong__5535A963");
        });

        modelBuilder.Entity<NhaCungCap>(entity =>
        {
            entity.HasKey(e => e.IdNhaCungCap).HasName("PK__nhaCungC__178CA80799F2E6AA");

            entity.ToTable("nhaCungCap");

            entity.Property(e => e.IdNhaCungCap).HasColumnName("idNhaCungCap");
            entity.Property(e => e.Sdt)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasColumnName("sdt");
            entity.Property(e => e.TenNhaCungCap)
                .HasMaxLength(200)
                .HasColumnName("tenNhaCungCap");
        });

        modelBuilder.Entity<NhanVien>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__NhanVien__CB9A1CDF0CD19694");

            entity.ToTable("NhanVien");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("userID");
            entity.Property(e => e.IdQuyen).HasColumnName("idQuyen");
            entity.Property(e => e.Sdt)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasColumnName("sdt");
            entity.Property(e => e.TenNv)
                .HasMaxLength(100)
                .HasColumnName("tenNV");

            entity.HasOne(d => d.IdQuyenNavigation).WithMany(p => p.NhanViens)
                .HasForeignKey(d => d.IdQuyen)
                .HasConstraintName("FK__NhanVien__idQuye__44FF419A");

            entity.HasOne(d => d.User).WithOne(p => p.NhanVien)
                .HasForeignKey<NhanVien>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NhanVien__userID__45F365D3");
        });

        modelBuilder.Entity<PhanHoi>(entity =>
        {
            entity.HasKey(e => e.IdPhanHoi).HasName("PK__phanHoi__9D16DD957DC39524");

            entity.ToTable("phanHoi");

            entity.Property(e => e.IdPhanHoi).HasColumnName("idPhanHoi");
            entity.Property(e => e.IdHocVien).HasColumnName("idHocVien");
            entity.Property(e => e.IdLopHoc).HasColumnName("idLopHoc");
            entity.Property(e => e.NgayPh)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("ngayPH");
            entity.Property(e => e.NoiDung).HasColumnName("noiDung");
            entity.Property(e => e.SoSao).HasColumnName("soSao");

            entity.HasOne(d => d.IdHocVienNavigation).WithMany(p => p.PhanHois)
                .HasForeignKey(d => d.IdHocVien)
                .HasConstraintName("FK__phanHoi__idHocVi__5CD6CB2B");

            entity.HasOne(d => d.IdLopHocNavigation).WithMany(p => p.PhanHois)
                .HasForeignKey(d => d.IdLopHoc)
                .HasConstraintName("FK__phanHoi__idLopHo__5DCAEF64");
        });

        modelBuilder.Entity<PhieuNhapHang>(entity =>
        {
            entity.HasKey(e => e.IdPhieuNhapHang).HasName("PK__phieuNha__9CE097CE97F112B9");

            entity.ToTable("phieuNhapHang");

            entity.Property(e => e.IdPhieuNhapHang).HasColumnName("idPhieuNhapHang");
            entity.Property(e => e.IdNhaCungCap).HasColumnName("idNhaCungCap");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("ngayTao");
            entity.Property(e => e.TongTien).HasColumnName("tongTien");
            entity.Property(e => e.UserId).HasColumnName("userID");

            entity.HasOne(d => d.IdNhaCungCapNavigation).WithMany(p => p.PhieuNhapHangs)
                .HasForeignKey(d => d.IdNhaCungCap)
                .HasConstraintName("FK__phieuNhap__idNha__6E01572D");

            entity.HasOne(d => d.User).WithMany(p => p.PhieuNhapHangs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__phieuNhap__userI__6D0D32F4");
        });

        modelBuilder.Entity<PhongHoc>(entity =>
        {
            entity.HasKey(e => e.IdPhong).HasName("PK__phongHoc__E540EED4C75C1C81");

            entity.ToTable("phongHoc");

            entity.Property(e => e.IdPhong).HasColumnName("idPhong");
            entity.Property(e => e.TenPhong)
                .HasMaxLength(100)
                .HasColumnName("tenPhong");
        });

        modelBuilder.Entity<PhuHuynh>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__PhuHuynh__CB9A1CDF63CC3DB1");

            entity.ToTable("PhuHuynh");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("userID");
            entity.Property(e => e.Avatar)
                .IsUnicode(false)
                .HasColumnName("avatar");
            entity.Property(e => e.GioiTinh)
                .HasMaxLength(20)
                .HasColumnName("gioiTinh");
            entity.Property(e => e.NgaySinh).HasColumnName("ngaySinh");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("ngayTao");
            entity.Property(e => e.Sdt)
                .HasMaxLength(12)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("sdt");
            entity.Property(e => e.TenPh)
                .HasMaxLength(100)
                .HasColumnName("tenPH");

            entity.HasOne(d => d.User).WithOne(p => p.PhuHuynh)
                .HasForeignKey<PhuHuynh>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PhuHuynh__userID__3C69FB99");
        });

        modelBuilder.Entity<Quyen>(entity =>
        {
            entity.HasKey(e => e.IdQuyen).HasName("PK__quyen__2EA0D08434E01760");

            entity.ToTable("quyen");

            entity.Property(e => e.IdQuyen).HasColumnName("idQuyen");
            entity.Property(e => e.TenQuyen)
                .HasMaxLength(50)
                .HasColumnName("tenQuyen");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__CB9A1CDFE708C3AA");

            entity.Property(e => e.UserId).HasColumnName("userID");
            entity.Property(e => e.IsHocVien).HasColumnName("isHocVien");
            entity.Property(e => e.Mail)
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("mail");
            entity.Property(e => e.MatKhau)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("matKhau");
            entity.Property(e => e.RefeshToken)
                .IsUnicode(false)
                .HasColumnName("refeshToken");
            entity.Property(e => e.RefeshTokenCreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("refeshTokenCreatedAt");
            entity.Property(e => e.RefreshTokenExpires)
                .HasDefaultValueSql("(dateadd(day,(7),getdate()))")
                .HasColumnType("datetime");
            entity.Property(e => e.Token)
                .IsUnicode(false)
                .HasColumnName("token");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
