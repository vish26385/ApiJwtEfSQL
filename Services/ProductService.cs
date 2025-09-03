using ApiJwtEfSQL.Models;
using ApiJwtEfSQL.Repositories;

namespace ApiJwtEfSQL.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }
        public async Task<List<Product>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllProductsAsync();
            return products;
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            return product;
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            var savedproduct = await _productRepository.CreateProductAsync(product);
            return savedproduct;
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            var productexist = await _productRepository.GetProductByIdAsync(product.Id);
            if (productexist == null) return false;

            await _productRepository.UpdateProductAsync(product);
            return true;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null) return false;

            await _productRepository.DeleteProductAsync(product);
            return true;
        }
    }
}
