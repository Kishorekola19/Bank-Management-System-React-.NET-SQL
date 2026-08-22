import React, { useState, useEffect } from 'react';
import api from '../services/api';
import { Users, Search, RefreshCw, ShieldCheck } from 'lucide-react';

export const CustomersPage = () => {
  const [customers, setCustomers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [searchTerm, setSearchTerm] = useState('');

  const fetchCustomers = async () => {
    setLoading(true);
    setError('');
    try {
      const res = await api.get('/customer');
      setCustomers(res.data);
    } catch (err) {
      setError('Failed to fetch customer directory.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchCustomers();
  }, []);

  const filteredCustomers = customers.filter(c =>
    (c.fullName || '').toLowerCase().includes(searchTerm.toLowerCase()) ||
    (c.email || '').toLowerCase().includes(searchTerm.toLowerCase()) ||
    (c.phone || '').toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 flex items-center gap-2">
            <Users className="w-7 h-7 text-blue-700" /> Customer Directory & KYC Registry
          </h1>
          <p className="text-sm text-slate-600 mt-1">
            Enterprise Client Master File & Compliance Status
          </p>
        </div>

        <button
          onClick={fetchCustomers}
          className="px-4 py-2.5 bg-white hover:bg-slate-50 text-slate-700 rounded-xl text-sm font-bold border border-slate-200 shadow-xs transition-all flex items-center gap-2"
        >
          <RefreshCw className="w-4 h-4" /> Refresh Directory
        </button>
      </div>

      <div className="bg-white border border-slate-200 rounded-2xl p-6 shadow-sm space-y-4">
        <div className="relative max-w-md">
          <input
            type="text"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            placeholder="Search by customer name, email, or phone..."
            className="w-full pl-9 pr-4 py-2 bg-white border border-slate-300 rounded-xl text-sm text-slate-900 focus:outline-none focus:border-blue-700"
          />
          <Search className="w-4 h-4 text-slate-400 absolute left-3 top-2.5" />
        </div>

        {filteredCustomers.length === 0 ? (
          <div className="text-center py-8 text-slate-500 text-sm font-medium">No customer records found.</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm text-slate-700">
              <thead className="bg-slate-50 text-xs font-bold uppercase text-slate-500 border-b border-slate-200">
                <tr>
                  <th className="py-3 px-4">ID</th>
                  <th className="py-3 px-4">Customer Name</th>
                  <th className="py-3 px-4">Email</th>
                  <th className="py-3 px-4">Phone</th>
                  <th className="py-3 px-4">Address</th>
                  <th className="py-3 px-4">KYC Status</th>
                  <th className="py-3 px-4">Registered Date</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {filteredCustomers.map((c) => (
                  <tr key={c.id} className="hover:bg-slate-50">
                    <td className="py-3 px-4 font-mono text-xs text-slate-500 font-bold">#{c.id}</td>
                    <td className="py-3 px-4 font-bold text-slate-900 flex items-center gap-2">
                      <div className="w-7 h-7 bg-blue-100 border border-blue-200 text-blue-800 rounded-full flex items-center justify-center text-xs font-bold">
                        {c.firstName.charAt(0)}{c.lastName.charAt(0)}
                      </div>
                      {c.fullName}
                    </td>
                    <td className="py-3 px-4 text-xs font-mono text-slate-600">{c.email}</td>
                    <td className="py-3 px-4 text-xs text-slate-700 font-medium">{c.phone}</td>
                    <td className="py-3 px-4 text-xs text-slate-600 font-medium">{c.address}</td>
                    <td className="py-3 px-4">
                      <span className="px-2.5 py-0.5 bg-emerald-50 border border-emerald-200 text-emerald-800 text-xs font-bold rounded-full flex items-center gap-1 w-max">
                        <ShieldCheck className="w-3 h-3" /> {c.kycStatus}
                      </span>
                    </td>
                    <td className="py-3 px-4 text-xs text-slate-500 font-mono">
                      {new Date(c.createdAt).toLocaleDateString()}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
};
